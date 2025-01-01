
# HerGunEnUcuz Product Search Plugin for nopCommerce

The HerGunEnUcuz Product Search Plugin enhances the default nopCommerce search capabilities by incorporating advanced search features that consider various product attributes including names, descriptions, categories, and manufacturers. This plugin also supports localized searches, ensuring results are relevant to diverse user bases.

## Features

- **Advanced Search Logic**: Scores products based on the occurrence of search terms across multiple fields.
- **Score-Based Filtering**: Prioritizes products with the highest relevance scores.
- **Localization Support**: Provides search results tailored to the user's locale.
- **Configurable**: Allows easy configuration through the nopCommerce administration panel.

## Installation

1. **Download the Plugin**: Clone this repository or download the latest release.
2. **Deploy the Plugin**: Copy the plugin files to the `Plugins` directory of your nopCommerce installation.
3. **Restart nopCommerce**: Restart your application to detect the new plugin.
4. **Install and Configure**: Navigate to `Configuration > Plugins` in the admin panel to install and configure the plugin.

## Usage

Once installed, the plugin replaces the default search functionality. Perform searches from the storefront, and the plugin will handle queries using its advanced logic.

### Using the Search API

For developers, use the `SearchProductsAsync` method to integrate advanced search functionality:

```python
var searchResults = await _productSearchPlugin.SearchProductsAsync("search query", true);
```

This method returns a list of product IDs that match the search criteria, scored and sorted by relevance.

## Uninstallation

1. **Navigate to the Plugins Page**: Find the plugin under `Configuration > Plugins`.
2. **Uninstall the Plugin**: Use the "Uninstall" button.
3. **Remove Plugin Files**: Delete the plugin files from your `Plugins` directory.
4. **Restart nopCommerce**: Restart your application to complete the process.

## Contributing

This plugin is open for development and the community is encouraged to contribute. Whether you wish to fix bugs, enhance functionalities, or suggest improvements, your input is welcome!

- **Fork and Clone**: Feel free to fork this repository, make improvements, and submit pull requests.
- **Issues and Suggestions**: If you have suggestions or encounter issues, please post them under the issues section of this repository.

## Support

For support, please open an issue on this GitHub repository or contact support at `info@hergunenucuz.com`.
