using Services;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using AGRA_EASY_MOBILE.Models;

namespace AGRA_EASY_MOBILE.Services
{
    public static class EasySession
    {
        public static ShoppingCartControllerSoapClient Client { get; private set; }
        public static string CurrentUrl { get; private set; }
        public static bool IsConnected { get; private set; } = false;
        public static string CurrentLogin { get; private set; } = string.Empty;
        public static global::Services.EasyAccount? CurrentAccount { get; private set; }
        public static bool IsCustomerBillingManager { get; private set; } = false;
        public static string LastConnectionDiagnosticText { get; private set; } = string.Empty;
        private static bool _applicationShellResetRequired;

        public static async Task<DeliveryLine[]> GetDeliveriesLinesAsync(ExpeditionFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetDeliveriesLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.ProductCode,
                    f.AccountCode,
                    f.ClientSorderCode,
                    f.SorderType,
                    f.ContainerNo,
                    f.DeliveryNumber,
                    f.ShowDetails);

                return response.GetDeliveriesLinesResult;
            });
        }

        public static async Task<DeliveryLine[]> GetDeliveryLinesAsync(string deliveryId, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetDeliveryLinesAsync(deliveryId, warehouse);
            });
        }

        public static async Task<byte[]> GetDeliveryDocumentAsync(string deliveryId, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetDeliveryDocumentAsync(deliveryId, warehouse);
                return response.GetDeliveryDocumentResult;
            });
        }


        public static async Task<SorderLine[]> GetSordersLinesAsync(ExpeditionFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetSordersLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ClientSorderCode,
                    f.SorderType,
                    f.ContainerNo,
                    f.DeliveryNumber,
                    f.ShowDetails);

                return response.GetSordersLinesResult;
            });
        }

        public static async Task<SorderLine[]> GetSorderLinesAsync(string sorderCode, string warehouse, string originalWarehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetSorderLinesAsync(sorderCode, warehouse, originalWarehouse);
            });
        }

        public static async Task<byte[]> GetSorderDocumentAsync(string sorderCode, string warehouse, string originalWarehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetSorderDocumentAsync(sorderCode, warehouse, originalWarehouse);
                return response.GetSorderDocumentResult;
            });
        }

        public static async Task<RuptureLine[]> GetRupturesLinesAsync(ExpeditionFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetRupturesLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ClientSorderCode,
                    f.SorderType,
                    f.DeliveryNumber,
                    f.ShowDetails);

                return response.GetRupturesLinesResult;
            });
        }

        public static async Task<RuptureLine[]> GetRuptureLinesAsync(string ruptureNumber, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetRuptureLinesAsync(ruptureNumber, warehouse);
            });
        }

        public static async Task<byte[]> GetRuptureDocumentAsync(string ruptureNumber, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetRuptureDocumentAsync(ruptureNumber, warehouse);
                return response.GetRuptureDocumentResult;
            });
        }

        public static async Task<ShippingNotice[]> GetShippingNoticeListAsync(ExpeditionFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetShippingNoticeListAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.ProductCode,
                    f.AccountCode,
                    f.ClientSorderCode,
                    f.SorderType,
                    f.ContainerNo,
                    f.DeliveryNumber);

                return response.GetShippingNoticeListResult;
            });
        }

        public static async Task<ReturnLine[]> GetReturnsLinesAsync(ReturnFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetReturnsLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ReturnStatus,
                    f.ProductStatus,
                    f.ReturnCode,
                    f.ReturnClientCode,
                    f.ShowDetails);

                return response.GetReturnsLinesResult;
            });
        }

        public static async Task<ReturnLine[]> GetReturnLinesAsync(string returnCode, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetReturnLinesAsync(returnCode, warehouse);
            });
        }

        public static async Task<byte[]> GetReturnDocumentAsync(string returnCode, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetReturnDocumentAsync(returnCode, warehouse);
                return response.GetReturnDocumentResult;
            });
        }

        public static async Task<RefusedReturnLine[]> GetRefusedReturnsLinesAsync(ReturnFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetRefusedReturnsLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ReturnCode,
                    f.ReturnClientCode);

                return response.GetRefusedReturnsLinesResult;
            });
        }

        public static async Task<RefusedReturnLine[]> GetRefusedReturnLineAsync(string refusedReturnLineCode, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetRefusedReturnLineAsync(refusedReturnLineCode, warehouse);
            });
        }

        public static async Task<string> UploadRefusedReturnPictureAsync(byte[] pictureByteTable, string receptionTracingId)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.UploadRefusedReturnPictureAsync(pictureByteTable, receptionTracingId);
                return response.UploadRefusedReturnPictureResult;
            });
        }

        public static async Task<SupplierRefundLine[]> GetSupplierRefundsLinesAsync(ReturnFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetSupplierRefundsLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ReturnCode,
                    f.ReturnClientCode,
                    f.WithTreated);

                return response.GetSupplierRefundsLinesResult;
            });
        }

        public static async Task<SupplierRefundLine[]> GetSupplierRefundLineAsync(string supplierRefundLineCode, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetSupplierRefundLineAsync(supplierRefundLineCode, warehouse);
            });
        }

        public static async Task<byte[]> GetSupplierRefundDocumentAsync(string supplierRefundLineCode, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetSupplierRefundDocumentAsync(supplierRefundLineCode, warehouse);
                return response.GetSupplierRefundDocumentResult;
            });
        }

        public static async Task<CustomerBillingLine[]> GetCustomerBillingsLinesAsync(CustomerBillingFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetCustomerBillingsLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.ProductCode,
                    f.AccountCode,
                    f.InvoicedAccountCode,
                    f.ClientSorderCode,
                    f.DeliveryNumber,
                    f.ShowDetails);

                return response.GetCustomerBillingsLinesResult;
            });
        }

        public static async Task<CustomerBillingLine[]> GetCustomerBillingLinesAsync(string customerBillingId, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetCustomerBillingLinesAsync(customerBillingId, warehouse);
            });
        }

        public static async Task<byte[]> GetCustomerBillingDocumentAsync(string customerBillingId, string warehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetCustomerBillingDocumentAsync(customerBillingId, warehouse);
                return response.GetCustomerBillingDocumentResult;
            });
        }

        public static async Task<InvoiceWaitingLine[]> GetInvoiceWaitingLinesAsync(ExpeditionFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetInvoiceWaitingLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ClientSorderCode,
                    f.DeliveryType,
                    f.ContainerNo,
                    f.DeliveryNumber);

                return response.GetInvoiceWaitingLinesResult;
            });
        }


        public static async Task<RefundWatingLine[]> GetRefundWaitingLinesAsync(ReturnFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetRefundWaitingLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ReturnCode,
                    f.ReturnClientCode);

                return response.GetRefundWaitingLinesResult;
            });
        }

        public static async Task<ShippingCostLine[]> GetShippingCostLinesAsync(AGRA_EASY_MOBILE.Models.ShippingCostFilter f)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await Client.GetShippingCostLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.Warehouse,
                    f.AccountCode,
                    f.ShippingCostDeliveryNumber,
                    f.TurnoverDeliveryNumber,
                    f.ShowDetails,
                    f.WithBilledLines,
                    f.WithExemptLines,
                    f.WithWaitingLines);

                return response.GetShippingCostLinesResult;
            });
        }



        public static async Task<ShippingWarning[]> GetShippingWarningListAsync(AGRA_EASY_MOBILE.Models.ShippingWarningFilter f, decimal? offset, decimal? count)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var accountCode = IsClient
                    ? CurrentAccount?.AccountCode
                    : f.AccountCode;

                var response = await Client.GetShippingWarningListAsync(
                    f.FirstDate,
                    f.LastDate,
                    accountCode ?? string.Empty,
                    f.ContainerNo ?? string.Empty,
                    f.Warehouse ?? string.Empty,
                    true,
                    offset,
                    count);

                return response.GetShippingWarningListResult;
            });
        }

        public static async Task<ShippingWarning[]> GetShippingWarningAsync(string shippingWarningId, string originalWarehouse)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetShippingWarningAsync(shippingWarningId, originalWarehouse);
            });
        }


        public static async Task<bool> IsReturnSystemsManagerAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsReturnSystemsManagerAsync();
            });
        }

        public static async Task<bool> IsReturnSystemsSuperManagerAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsReturnSystemsSuperManagerAsync();
            });
        }

        public static async Task<ClientAccount> SetReturnBasketAccountCodeAsync(string accountCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.SetReturnBasketAccountCodeAsync(accountCode);
            });
        }

        public static async Task<ReturnableArticle[]> FindReturnableArticleListAsync(string keyword)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.FindReturnableArticleListAsync(keyword);
            });
        }

        public static async Task<ReturnableArticle[]> FindProductCodeListAsync(string keyword, bool onlyActiveProduct, bool isGenCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.FindProductCodeListAsync(keyword, onlyActiveProduct, isGenCode);
            });
        }

        public static async Task<DeliveryLinesStatus[]> GetArticlesReturnableDeliveryLinesAsync(string productCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetArticlesReturnableDeliveryLinesAsync(productCode);
            });
        }

        public static async Task<string> GetReturnBasketAccountCodeAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetReturnBasketAccountCodeAsync();
            });
        }

        public static async Task<string> GetDepotSupplierCodeAsync(string accountCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetDepotSupplierCodeAsync(accountCode);
            });
        }

        public static async Task<Warehouse[]> GetWarehousesListV2Async()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetWarehousesListV2Async();
            });
        }

        public static async Task AddReturnBasketLineAsync(string warehouse, string contretuIndex, decimal quantity, bool isGarantie, string motif, string originalProduct)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.AddReturnBasketLineAsync(warehouse, contretuIndex, quantity, isGarantie, motif, originalProduct);
                return true;
            });
        }

        public static async Task<ReturnBasketLine[]> GetReturnBasketLinesAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetReturnBasketLinesAsync();
            });
        }

        public static async Task DeleteReturnBasketLineAsync(string returnNoticeLineId)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.DeleteReturnBasketLineAsync(returnNoticeLineId);
                return true;
            });
        }

        public static async Task<ReturnNoticeKey> ReturnBasketCheckOutAsync(string returnClientCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.ReturnBasketCheckOutAsync(returnClientCode);
            });
        }


        public static bool IsAdministrator
        {
            get
            {
                return string.Equals(CurrentAccount?.Type ?? string.Empty, "Administrateur", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static bool IsClient
        {
            get
            {
                return string.Equals(CurrentAccount?.Type ?? string.Empty, "Client", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static async Task<string> GetOrderBasketAccountCodeAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetOrderBasketAccountCodeAsync();
            });
        }

        public static async Task<ClientAccount> SetOrderBasketAccountCodeAsync(string accountCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.SetOrderBasketAccountCodeAsync(accountCode);
            });
        }

        public static async Task<ClientAccount[]> FindClientAccountAsync(string keyword)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.FindClientAccountAsync(keyword);
            });
        }

        public static async Task<bool> IsSalesConditionManagerAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsSalesConditionManagerAsync();
            });
        }

        public static async Task<bool> IsClientOrderManagerAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsClientOrderManagerAsync();
            });
        }

        public static async Task<bool> IsOrderManagerAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsOrderManagerAsync();
            });
        }

        public static async Task<bool> IsPreOrderManagerAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsPreOrderManagerAsync();
            });
        }

        public static async Task<bool> IsCatalogManagerAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsCatalogManagerAsync();
            });
        }

        public static async Task<bool> AllowChangeDeliveryAddressAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.AllowChangeDeliveryAddressAsync();
            });
        }

        public static async Task<bool> IsExpressSystemBlockedAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsExpressSystemBlockedAsync();
            });
        }

        public static async Task<bool> IsMagasinSystemBlockedAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsMagasinSystemBlockedAsync();
            });
        }

        public static async Task<CatalogArticle[]> FindArticleListAsync(string filter, bool showAll)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.FindArticleListAsync(filter, showAll);
            });
        }

        public static async Task<Vehicule> GetVehiculeFromImmatriculationAsync(string immatriculation)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetVehiculeFromImmatriculationAsync(immatriculation);
            });
        }

        public static async Task<VehicleSearchResult> GetKTypeAlternativeAsync(string kTypNo, string treeNodeId)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetKTypeAlternativeAsync(kTypNo, treeNodeId);
            });
        }

        public static async Task<ArticlePriceAndStock[]> GetAllConditionForArticleAndAlternativeAsync(string shortProductCode, string supplierCode, string catalog, bool showAll)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetAllConditionForArticleAndAlternativeAsync(shortProductCode, supplierCode, catalog, showAll);
            });
        }

        public static async Task<ArticlFile[]> GetArticleFileListAsync(string brandNo, string articleNo)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetArticleFileListAsync(brandNo, articleNo);
            });
        }

        public static async Task<ArticleReference[]> GetArticleReferenceListAsync(string brandNo, string articleNo)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetArticleReferenceListAsync(brandNo, articleNo);
            });
        }

        public static async Task<ArticlVehicle[]> GetArticleVehicleListAsync(string brandNo, string articleNo)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetArticleVehicleListAsync(brandNo, articleNo);
            });
        }

        public static async Task<ArticleAttribute[]> GetArticleAttributeListAsync(string brandNo, string articleNo)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetArticleAttributeListAsync(brandNo, articleNo);
            });
        }

        public static async Task<ArticlePriceAndStock[]> GetLocalConditionForArticleListAsync(GenericArticle[] articleList)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetLocalConditionForArticleListAsync(articleList);
            });
        }

        public static async Task<ArticlePriceAndStock> GetAllConditionForArticleAsync(string shortProductCode, string supplierCode, string catalog, int quantitative)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetAllConditionForArticleAsync(shortProductCode, supplierCode, catalog, quantitative);
            });
        }

        public static async Task<ArticlePriceAndStock> GetLocalConditionForArticleAsync(string shortProductCode, string supplierCode, string catalog, int quantitative)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetLocalConditionForArticleAsync(shortProductCode, supplierCode, catalog, quantitative);
            });
        }

        public static async Task AddArticleListToWarehouseShoppingCartAsync(OrderLine[] orderLines)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.AddArticleListToWarehouseShoppingCartAsync(orderLines);
                return true;
            });
        }


        public static async Task<int> GetExternalStockStatusAsync(string productCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetExternalStockStatusAsync(productCode);
            });
        }

        public static async Task<ExternalSorderReturnBack> SendExternalCommandAsync(string productCode, int quantity, string clientSorderNumber)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.SendExternalCommandAsync(productCode, quantity, clientSorderNumber);
            });
        }

        public static async Task<SorderBasketLine[]> GetOrderBasketLinesAsync(bool withConsigneAndEcotaxe, bool onlyLocalDisponibility, string warehouseCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetOrderBasketLinesAsync(withConsigneAndEcotaxe, onlyLocalDisponibility, warehouseCode);
            });
        }

        public static async Task<ShoppingCartWarehouseInformation[]> GetShoppingCartDashboardAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetShoppingCartDashboardAsync();
            });
        }

        public static async Task<ShoppingCartWarehouseInformation[]> GetWarehouseDashboardAsync(bool forAllPlatform)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetWarehouseDashboardAsync(forAllPlatform);
            });
        }

        public static async Task<int> GetShoppingCartUnitCountAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetShoppingCartUnitCountAsync();
            });
        }

        public static async Task<decimal> GetTotalSorderCoastAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetTotalSorderCoastAsync();
            });
        }

        public static async Task<decimal> GetGarageTotalSorderCoastAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetGarageTotalSorderCoastAsync();
            });
        }

        public static async Task DeleteShoppingCartLineAsync(string shoppingCartLine)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.DeleteShoppingCartLineAsync(shoppingCartLine);
                return true;
            });
        }

        public static async Task UpdateShoppingCartLineQuantityAsync(string productCode, string warehouseCode, string newQuantity)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.UpdateShoppingCartLineQuantityAsync(productCode, warehouseCode, newQuantity);
                return true;
            });
        }

        public static async Task<bool> IsPromotionActiveAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.IsPromotionActiveAsync();
            });
        }

        public static async Task<PromotionBonusLine[]> TreatePromotionWithPromotionalCodeAsync(string promotionalCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.TreatePromotionWithPromotionalCodeAsync(promotionalCode);
            });
        }

        public static async Task ApplicatePromotionAsync(string productCode)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.ApplicatePromotionAsync(productCode);
                return true;
            });
        }

        public static async Task CancelPromotionAsync()
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.CancelPromotionAsync();
                return true;
            });
        }

        public static async Task ClearShoppingCartAsync()
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.ClearShoppingCartAsync();
                return true;
            });
        }

        public static async Task<ShoppingCartResponse[]> WarehouseShoppingCartCheckOutAsync(string type, string sorderNumber)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.WarehouseShoppingCartCheckOutAsync(type, sorderNumber);
            });
        }

        public static async Task<ShoppingCartResponse[]> SelectedWarehouseShoppingCartCheckOutAsync(string type, string sorderNumber, string warehouseCode)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.SelectedWarehouseShoppingCartCheckOutAsync(type, sorderNumber, warehouseCode);
            });
        }

        public static async Task<DelivryAddress> GetDelivryAddressAsync()
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                return await Client.GetDelivryAddressAsync();
            });
        }

        public static async Task SetDelivryAddressAsync(string line1, string line2, string zip, string city, string country, string contactName, string contactTel, string contactFax, string contactMail)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.SetDelivryAddressAsync(line1, line2, zip, city, country, contactName, contactTel, contactFax, contactMail);
                return true;
            });
        }

        public static async Task SetDefaultDelivryAddressAsync()
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.SetDefaultDelivryAddressAsync();
                return true;
            });
        }

        public static async Task SetDelivryDateAsync(DateTime delivryDate)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.SetDelivryDateAsync(delivryDate);
                return true;
            });
        }

        public static async Task ClearDelivryDateAsync()
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.ClearDelivryDateAsync();
                return true;
            });
        }

        public static async Task SetLotsNumberAsync(string lotsNumber)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.SetLotsNumberAsync(lotsNumber);
                return true;
            });
        }

        public static async Task ClearLotsNumberAsync()
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await Client.ClearLotsNumberAsync();
                return true;
            });
        }

        public static async Task PerformAutoLoginAsync()
        {
            string user = Preferences.Default.Get("UserLogin", string.Empty);
            string pass = Preferences.Default.Get("UserPassword", string.Empty);

            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
            {
                await ValidateAndNavigateAsync();
            }
            else
            {
                await ShowLoginPageAsync();
            }
        }

        public static async Task<bool> ValidateAndNavigateAsync()
        {
            try
            {
                bool connected = await OpenConnectionAsync();
                if (connected)
                {
                    await ShowShellAfterSuccessfulConnectionAsync("//home");
                    return true;
                }

                await ShowLoginPageAsync();
                return false;
            }
            catch
            {
                await ShowLoginPageAsync();
                return false;
            }
        }

        public static async Task<bool> OpenConnectionAsync()
        {
            string warehouse = Preferences.Default.Get("SelectedWarehouseName", "Meyzieu");
            bool useSecondary = Preferences.Default.Get("UseSecondaryLink", false);
            string user = Preferences.Default.Get("UserLogin", "");
            string pass = Preferences.Default.Get("UserPassword", "");
            LastConnectionDiagnosticText = string.Empty;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                return false;

            var previousAccount = CurrentAccount;
            string previousLogin = CurrentLogin;

            BasicHttpBinding? binding = null;

            try
            {
                if (Client != null && (IsConnected || CurrentAccount != null))
                    await CloseCurrentClientBeforeNewConnectionAsync();

                CurrentUrl = await GetUrlFromPlatformsXml(warehouse, useSecondary);

                binding = CurrentUrl.ToLower().StartsWith("https")
                    ? new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                    : new BasicHttpBinding(BasicHttpSecurityMode.None);

                binding.MaxReceivedMessageSize = int.MaxValue;
                binding.AllowCookies = true;

                Client = new ShoppingCartControllerSoapClient(binding, new EndpointAddress(CurrentUrl));
                CurrentAccount = await Client.ConnexionAsync(user, pass, "MAUI_AGRA_V10");

                IsConnected = CurrentAccount != null;
                if (IsConnected)
                {
                    CurrentLogin = user;
                    try
                    {
                        IsCustomerBillingManager = await Client.IsCustomerBillingManagerAsync();
                    }
                    catch
                    {
                        IsCustomerBillingManager = false;
                    }
                }
                else
                {
                    IsCustomerBillingManager = false;
                }

                if (IsConnected && HasConnectedUserChanged(previousAccount, CurrentAccount, previousLogin, CurrentLogin))
                {
                    GlobalState.ClearExpeditionFilter();
                    GlobalState.ClearReturnFilter();
                    GlobalState.ClearCustomerBillingFilter();
                    _applicationShellResetRequired = true;
                }

                return IsConnected;
            }
            catch (Exception ex)
            {
                CaptureConnectionDiagnostic(ex, warehouse, useSecondary, CurrentUrl, binding);
                throw;
            }
        }

        private static void CaptureConnectionDiagnostic(Exception exception, string warehouse, bool useSecondary, string? url, BasicHttpBinding? binding)
        {
            LastConnectionDiagnosticText = BuildConnectionDiagnostic(exception, warehouse, useSecondary, url, binding);
            System.Diagnostics.Debug.WriteLine(LastConnectionDiagnosticText);
        }

        private static string BuildConnectionDiagnostic(Exception exception, string warehouse, bool useSecondary, string? url, BasicHttpBinding? binding)
        {
            var sb = new StringBuilder();
            var uri = Uri.TryCreate(url ?? string.Empty, UriKind.Absolute, out var parsedUri) ? parsedUri : null;

            sb.AppendLine("Diagnostic temporaire connexion SOAP");
            sb.AppendLine($"Date locale : {DateTime.Now:O}");
            sb.AppendLine($"Plateforme MAUI : {DeviceInfo.Current.Platform}");
            sb.AppendLine($"Version OS : {DeviceInfo.Current.VersionString}");
            sb.AppendLine($"Modele appareil : {DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}");
            sb.AppendLine($"Application : {AppInfo.Current.Name} {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})");
            sb.AppendLine();
            sb.AppendLine("Contexte appel");
            sb.AppendLine($"Entrepot selectionne : {warehouse}");
            sb.AppendLine($"Lien secondaire : {useSecondary}");
            sb.AppendLine($"URL appelee : {url ?? "(non resolue)"}");
            sb.AppendLine($"Protocole URL : {uri?.Scheme ?? "(inconnu)"}");
            sb.AppendLine($"Host : {uri?.Host ?? "(inconnu)"}");
            sb.AppendLine($"Binding SOAP : {binding?.GetType().Name ?? "(non cree)"}");
            sb.AppendLine($"Securite binding : {binding?.Security.Mode.ToString() ?? "(inconnue)"}");
            sb.AppendLine($"AllowCookies : {binding?.AllowCookies.ToString() ?? "(inconnu)"}");
            sb.AppendLine($"MaxReceivedMessageSize : {binding?.MaxReceivedMessageSize.ToString() ?? "(inconnu)"}");
            sb.AppendLine();
            sb.AppendLine("Statut reseau");
            sb.AppendLine($"NetworkAccess : {Connectivity.Current.NetworkAccess}");
            sb.AppendLine($"ConnectionProfiles : {string.Join(", ", Connectivity.Current.ConnectionProfiles)}");
            sb.AppendLine();
            sb.AppendLine("Exception complete");
            AppendException(sb, exception, 0);

            return sb.ToString();
        }

        private static void AppendException(StringBuilder sb, Exception exception, int level)
        {
            string prefix = level == 0 ? string.Empty : $"Inner exception niveau {level} - ";
            sb.AppendLine($"{prefix}Type : {exception.GetType().FullName}");
            sb.AppendLine($"{prefix}Message : {exception.Message}");

            if (exception is FaultException faultException)
            {
                sb.AppendLine($"{prefix}FaultCode : {faultException.Code}");
                sb.AppendLine($"{prefix}FaultReason : {faultException.Reason}");
            }

            sb.AppendLine($"{prefix}StackTrace :");
            sb.AppendLine(exception.StackTrace ?? "(aucune stack trace)");

            if (exception.InnerException != null)
            {
                sb.AppendLine();
                AppendException(sb, exception.InnerException, level + 1);
            }
        }

        public static async Task CloseConnectionAsync()
        {
            try
            {
                if (Client != null)
                    await Client.CloseAsync();
            }
            finally
            {
                Client = null;
                CurrentAccount = null;
                CurrentLogin = string.Empty;
                IsConnected = false;
                IsCustomerBillingManager = false;
            }
        }

        private static async Task CloseCurrentClientBeforeNewConnectionAsync()
        {
            try
            {
                if (Client != null)
                    await Client.CloseAsync();
            }
            catch
            {
            }
            finally
            {
                Client = null;
                CurrentAccount = null;
                CurrentLogin = string.Empty;
                IsConnected = false;
                IsCustomerBillingManager = false;
            }
        }

        private static bool HasConnectedUserChanged(global::Services.EasyAccount? previousAccount, global::Services.EasyAccount? newAccount, string previousLogin, string newLogin)
        {
            if (previousAccount == null && newAccount == null)
                return false;

            if (previousAccount == null && newAccount != null)
                return true;

            if (previousAccount != null && newAccount == null)
                return true;

            return !string.Equals(previousLogin ?? string.Empty, newLogin ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(previousAccount?.Type ?? string.Empty, newAccount?.Type ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(previousAccount?.AccountCode ?? string.Empty, newAccount?.AccountCode ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(previousAccount?.Warehouse ?? string.Empty, newAccount?.Warehouse ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public static Task ShowShellAfterSuccessfulConnectionAsync(string route)
            => NavigateAfterSuccessfulConnectionAsync(route);

        public static async Task ShowLoginPageAsync()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var loginPage = new LoginView();
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window != null)
                    window.Page = loginPage;
                else if (Application.Current != null)
                    Application.Current.MainPage = loginPage;
            });
        }

        private static async Task NavigateAfterSuccessfulConnectionAsync(string route)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var window = Application.Current?.Windows.FirstOrDefault();
                var currentPage = window?.Page ?? Application.Current?.MainPage;
                var currentShell = currentPage as AppShell;

                if (currentShell != null && !_applicationShellResetRequired)
                {
                    await currentShell.GoToAsync(route);
                    ShippingWarningNotificationService.RequestImmediateCheck();
                    return;
                }

                _applicationShellResetRequired = false;

                var newShell = new AppShell();
                if (window != null)
                    window.Page = newShell;
                else if (Application.Current != null)
                    Application.Current.MainPage = newShell;

                await newShell.GoToAsync(route);
                ShippingWarningNotificationService.RequestImmediateCheck();
            });
        }

        public static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> serviceCall)
        {
            try
            {
                if (Client == null)
                    await OpenConnectionAsync();

                return await serviceCall();
            }
            catch (Exception ex)
            {
                if (await TryRecoverConnectionAsync(ex))
                    return await serviceCall();

                throw;
            }
        }

        public static async Task<bool> TryRecoverConnectionAsync(Exception exception)
        {
            bool sessionStillConnected = false;

            if (Client == null)
                return false;

            try
            {
                sessionStillConnected = await Client.IsConnectedAsync();
            }
            catch
            {
                sessionStillConnected = false;
            }

            if (sessionStillConnected)
                return false;

            try
            {
                if (await OpenConnectionAsync())
                    return true;
            }
            catch
            {
            }

            await ShowLoginPageAsync();

            throw new OperationCanceledException("La session a expiré et la reconnexion a échoué.", exception);
        }

        private static async Task<string> GetUrlFromPlatformsXml(string warehouseName, bool secondary)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("platforms.xml");
                var doc = XDocument.Load(stream);
                var node = doc.Descendants("Warehouse").FirstOrDefault(x => x.Attribute("Name")?.Value == warehouseName);
                return (secondary ? node?.Attribute("Secondary")?.Value : node?.Attribute("Primary")?.Value)
                       ?? "http://security.groupe-agra.fr/easy/services/ShoppingCartController.asmx";
            }
            catch
            {
                return "http://security.groupe-agra.fr/easy/services/ShoppingCartController.asmx";
            }
        }
    }
}
