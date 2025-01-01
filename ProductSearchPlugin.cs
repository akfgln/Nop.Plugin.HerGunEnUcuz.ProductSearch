using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;

namespace Nop.Plugin.HerGunEnUcuz.ProductSearch
{
    public class ProductSearchPlugin : BasePlugin, ISearchProvider
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductAttributeMapping> _productProductAttributeMappingRepository;
        private readonly IRepository<ProductProductTagMapping> _productProductTagMappingRepository;

        public ProductSearchPlugin(
            CatalogSettings catalogSettings,
            ISettingService settingService,
            ILogger logger,
            ILocalizationService localizationService,
            IRepository<Category> categoryRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductAttributeMapping> productProductAttributeMappingRepository,
            IRepository<ProductProductTagMapping> productProductTagMappingRepository)
        {
            _catalogSettings = catalogSettings;
            _settingService = settingService;
            _logger = logger;
            _localizationService = localizationService;
            _categoryRepository = categoryRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _manufacturerRepository = manufacturerRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productAttributeRepository = productAttributeRepository;
            _productTagRepository = productTagRepository;
            _productProductAttributeMappingRepository = productProductAttributeMappingRepository;
            _productProductTagMappingRepository = productProductTagMappingRepository;

        }
        public override async Task InstallAsync()
        {

            await _settingService.SaveSettingAsync(new ProductSearchSettings
            {
                OldActiveSearchProviderSystemName = _catalogSettings.ActiveSearchProviderSystemName
            });

            _catalogSettings.ActiveSearchProviderSystemName = "HerGunEnUcuz.ProductSearch";
            _settingService.SaveSetting(_catalogSettings);


            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            var settings = _settingService.LoadSetting<ProductSearchSettings>();
            if (settings != null)
            {
                _catalogSettings.ActiveSearchProviderSystemName = settings.OldActiveSearchProviderSystemName;
                _settingService.SaveSetting(_catalogSettings);
            }

            await _settingService.DeleteSettingAsync<ProductSearchSettings>();

            await base.UninstallAsync();
        }

        public async Task<List<int>> SearchProductsAsync(string keywords, bool isLocalized)
        {
            if (string.IsNullOrEmpty(keywords))
                return new List<int>();

            var searchTerms = keywords.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            searchTerms.Insert(0, keywords);

            var products = _productRepository.Table.AsQueryable();
            var categories = _categoryRepository.Table.AsQueryable();
            var manufacturers = _manufacturerRepository.Table.AsQueryable();
            var attributes = _productAttributeRepository.Table.AsQueryable();
            var tags = _productTagRepository.Table.AsQueryable();
            var productCategories = _productCategoryRepository.Table.AsQueryable();
            var productManufacturers = _productManufacturerRepository.Table.AsQueryable();
            var productAttributes = _productProductAttributeMappingRepository.Table.AsQueryable();
            var productTags = _productProductTagMappingRepository.Table.AsQueryable();
            var localizedProperties = _localizedPropertyRepository.Table.AsQueryable();

            // Base product search with scoring
            var productScores = products.Select(p => new
            {
                ProductId = p.Id,
                Score = searchTerms.Count(term => p.Name.ToLower().Contains(term) ||
                                                   p.ShortDescription.ToLower().Contains(term) ||
                                                   p.FullDescription.ToLower().Contains(term))
            }).Where(p => p.Score > 0);

            // Category scoring
            var categoryScores = productCategories.Join(categories, pc => pc.CategoryId, c => c.Id,
                                                        (pc, c) => new
                                                        {
                                                            ProductId = pc.ProductId,
                                                            Score = searchTerms.Count(term => c.Name.ToLower().Contains(term))
                                                        }).Where(p => p.Score > 0);

            // Manufacturer scoring
            var manufacturerScores = productManufacturers.Join(manufacturers, pm => pm.ManufacturerId, m => m.Id,
                                                               (pm, m) => new
                                                               {
                                                                   ProductId = pm.ProductId,
                                                                   Score = searchTerms.Count(term => m.Name.ToLower().Contains(term))
                                                               }).Where(p => p.Score > 0);

            //// Attribute scoring
            //var attributeScores = productAttributes.Join(attributes, pa => pa.ProductAttributeId, a => a.Id,
            //                                             (pa, a) => new
            //                                             {
            //                                                 ProductId = pa.ProductId,
            //                                                 Score = searchTerms.Count(term => a.Name.ToLower().Contains(term))
            //                                             }).Where(p => p.Score > 0);

            //// Tag scoring
            //var tagScores = productTags.Join(tags, pt => pt.ProductTagId, t => t.Id,
            //                                 (pt, t) => new
            //                                 {
            //                                     ProductId = pt.ProductId,
            //                                     Score = searchTerms.Count(term => t.Name.ToLower().Contains(term))
            //                                 }).Where(p => p.Score > 0);

            // Combine scores from all sources
            var combinedScores = productScores.Concat(categoryScores).Concat(manufacturerScores)
                                              //.Concat(attributeScores).Concat(tagScores)
                                              .GroupBy(x => x.ProductId)
                                              .Select(g => new { ProductId = g.Key, TotalScore = g.Sum(x => x.Score) })
                                              .ToList();

            // Determine the top two distinct scores
            var topScores = combinedScores.Select(x => x.TotalScore).Distinct().OrderByDescending(x => x).Take(1).ToList();

            // Select products with top two scores
            var filteredProductIds = combinedScores.Where(x => topScores.Contains(x.TotalScore))
                                                   .OrderByDescending(x => x.TotalScore)
                                                   .Select(x => x.ProductId)
                                                   .Distinct()
                                                   .ToList();

            // Include localized search if applicable
            if (isLocalized)
            {
                var localizedScores = localizedProperties
                    .Where(lp => lp.LocaleKeyGroup == nameof(Product) && searchTerms.Any(term => lp.LocaleValue.ToLower().Contains(term)))
                    .Select(lp => new { ProductId = lp.EntityId, Score = 1 });

                var adjustedLocalizedScores = localizedScores.Select(x => new { ProductId = x.ProductId, TotalScore = x.Score });

                filteredProductIds = filteredProductIds.Concat(adjustedLocalizedScores.Select(x => x.ProductId)).Distinct().ToList();
            }
            // Log information for debugging
            var lmessage = $"ProductSearch Plugin keyword: {keywords} product count : {filteredProductIds.Count()} top product : {combinedScores.FirstOrDefault(p => p.ProductId == filteredProductIds.FirstOrDefault())?.ProductId ?? 0} top product score : {combinedScores.FirstOrDefault(p => p.ProductId == filteredProductIds.FirstOrDefault())?.TotalScore ?? 0}";
            _logger.Information(lmessage);


            return filteredProductIds;
        }

    }
}
