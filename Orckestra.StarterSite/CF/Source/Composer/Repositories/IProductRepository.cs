using System.Collections.Generic;
using System.Threading.Tasks;
using Orckestra.Composer.Parameters;
using Orckestra.Overture.ServiceModel.Metadata;
using Orckestra.Overture.ServiceModel.Products;
using Orckestra.Overture.ServiceModel.Search;

namespace Orckestra.Composer.Repositories
{
    /// <summary>
    /// Abstraction for the repository that will be in charge of retrieving products.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Gets a product.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>
        /// Instance of <see cref="Orckestra.Overture.ServiceModel.Products.Product" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">param</exception>
        /// <exception cref="System.ArgumentException"></exception>
        Task<Overture.ServiceModel.Products.Product> GetProductAsync(GetProductParam param);

        /// <summary>
        /// A definition is a list of properties that is allowed to be set to the instance of product or category being used
        /// </summary>
        /// <param name="param">Retrieve the entity definition defined in the system related to the Name parameter specified in the request</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">param</exception>
        Task<ProductDefinition> GetProductDefinitionAsync(GetProductDefinitionParam param);

        /// <summary>
        /// Gets the products price.
        /// </summary>
        /// <param name="productIds">The product ids.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        Task<List<ProductPrice>> CalculatePricesAsync(List<string> productIds, string scope);

        /// <summary>
        /// Gets the effective price info for a given product.
        /// </summary>
        /// <param name="productId">The product id.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        Task<EffectivePriceEntryInfo> GetEffectivePrice(string productId, string scope);

        /// <summary>
        /// Search products by ids
        /// </summary>
        /// <param name="productIds">The product id.</param>
        /// <returns></returns>
        Task<SearchResult> SearchProductByIdsAsync(List<string> productIds, string scope, string cultureName);

        /// <summary>
        /// Get image url for product
        /// </summary>
        /// <param name="productId">The product Id.</param>
        /// <param name="variantId">The product variant Id.</param>
        /// <returns></returns>
        Task<string> GetImageUrlAsync(string productId, string variantId, string scope);
    }
}