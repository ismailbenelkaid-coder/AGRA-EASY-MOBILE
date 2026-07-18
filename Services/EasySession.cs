using Services;
using System.ServiceModel;
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
        private static EasySoapManualClient? ManualClient;
        private static bool _applicationShellResetRequired;
        private static bool UseManualSoap => OperatingSystem.IsIOS();

        public static async Task<DeliveryLine[]> GetDeliveriesLinesAsync(ExpeditionFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetDeliveriesLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ClientSorderCode", f.ClientSorderCode), new SoapParameter("SorderType", f.SorderType), new SoapParameter("ContainerNo", f.ContainerNo), new SoapParameter("DeliveryNumber", f.DeliveryNumber), new SoapParameter("_withDetail", f.ShowDetails));
        }

        public static async Task<DeliveryLine[]> GetDeliveryLinesAsync(string deliveryId, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetDeliveryLinesAsync(deliveryId, warehouse);
            }, "GetDeliveryLines", new SoapParameter("DeliveryNumber", deliveryId), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<byte[]> GetDeliveryDocumentAsync(string deliveryId, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetDeliveryDocumentAsync(deliveryId, warehouse);
                return response.GetDeliveryDocumentResult;
            }, "GetDeliveryDocument", new SoapParameter("DeliveryNumber", deliveryId), new SoapParameter("Warehouse", warehouse));
        }


        public static async Task<SorderLine[]> GetSordersLinesAsync(ExpeditionFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetSordersLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("ClientSorderCode", f.ClientSorderCode), new SoapParameter("SorderType", f.SorderType), new SoapParameter("ContainerNo", f.ContainerNo), new SoapParameter("DeliveryOrRuptureNumber", f.DeliveryNumber), new SoapParameter("_withDetail", f.ShowDetails));
        }

        public static async Task<SorderLine[]> GetSorderLinesAsync(string sorderCode, string warehouse, string originalWarehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetSorderLinesAsync(sorderCode, warehouse, originalWarehouse);
            }, "GetSorderLines", new SoapParameter("EasySorderCode", sorderCode), new SoapParameter("Warehouse", warehouse), new SoapParameter("OriginalWarehouse", originalWarehouse));
        }

        public static async Task<byte[]> GetSorderDocumentAsync(string sorderCode, string warehouse, string originalWarehouse)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetSorderDocumentAsync(sorderCode, warehouse, originalWarehouse);
                return response.GetSorderDocumentResult;
            }, "GetSorderDocument", new SoapParameter("SorderCode", sorderCode), new SoapParameter("Warehouse", warehouse), new SoapParameter("OriginalWarehouse", originalWarehouse));
        }

        public static async Task<RuptureLine[]> GetRupturesLinesAsync(ExpeditionFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetRupturesLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("ClientSorderCode", f.ClientSorderCode), new SoapParameter("SorderType", f.SorderType), new SoapParameter("RuptureNumber", f.DeliveryNumber), new SoapParameter("_withDetail", f.ShowDetails));
        }

        public static async Task<RuptureLine[]> GetRuptureLinesAsync(string ruptureNumber, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetRuptureLinesAsync(ruptureNumber, warehouse);
            }, "GetRuptureLines", new SoapParameter("RuptureNumber", ruptureNumber), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<byte[]> GetRuptureDocumentAsync(string ruptureNumber, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetRuptureDocumentAsync(ruptureNumber, warehouse);
                return response.GetRuptureDocumentResult;
            }, "GetRuptureDocument", new SoapParameter("RuptureNumber", ruptureNumber), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<ShippingNotice[]> GetShippingNoticeListAsync(ExpeditionFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetShippingNoticeList", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ClientSorderCode", f.ClientSorderCode), new SoapParameter("SorderType", f.SorderType), new SoapParameter("ContainerNo", f.ContainerNo), new SoapParameter("DeliveryNumber", f.DeliveryNumber));
        }

        public static async Task<ReturnLine[]> GetReturnsLinesAsync(ReturnFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetReturnsLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("ReturnStatus", f.ReturnStatus), new SoapParameter("ProductStatus", f.ProductStatus), new SoapParameter("ReturnCode", f.ReturnCode), new SoapParameter("ReturnClientCode", f.ReturnClientCode), new SoapParameter("WithDetail", f.ShowDetails));
        }

        public static async Task<ReturnLine[]> GetReturnLinesAsync(string returnCode, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetReturnLinesAsync(returnCode, warehouse);
            }, "GetReturnLines", new SoapParameter("ReturnCode", returnCode), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<byte[]> GetReturnDocumentAsync(string returnCode, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetReturnDocumentAsync(returnCode, warehouse);
                return response.GetReturnDocumentResult;
            }, "GetReturnDocument", new SoapParameter("ReturnCode", returnCode), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<RefusedReturnLine[]> GetRefusedReturnsLinesAsync(ReturnFilter f)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetRefusedReturnsLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ReturnCode,
                    f.ReturnClientCode);

                return response.GetRefusedReturnsLinesResult;
            }, "GetRefusedReturnsLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("ReturnCode", f.ReturnCode), new SoapParameter("ReturnClientCode", f.ReturnClientCode));
        }

        public static async Task<RefusedReturnLine[]> GetRefusedReturnLineAsync(string refusedReturnLineCode, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetRefusedReturnLineAsync(refusedReturnLineCode, warehouse);
            }, "GetRefusedReturnLine", new SoapParameter("RefusedReturnLineCode", refusedReturnLineCode), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<string> UploadRefusedReturnPictureAsync(byte[] pictureByteTable, string receptionTracingId)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.UploadRefusedReturnPictureAsync(pictureByteTable, receptionTracingId);
                return response.UploadRefusedReturnPictureResult;
            }, "UploadRefusedReturnPicture", new SoapParameter("PictureByteTable", pictureByteTable), new SoapParameter("ReceptionTracingId", receptionTracingId));
        }

        public static async Task<SupplierRefundLine[]> GetSupplierRefundsLinesAsync(ReturnFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetSupplierRefundsLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("ReturnCode", f.ReturnCode), new SoapParameter("ReturnClientCode", f.ReturnClientCode), new SoapParameter("WithTreated", f.WithTreated));
        }

        public static async Task<SupplierRefundLine[]> GetSupplierRefundLineAsync(string supplierRefundLineCode, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetSupplierRefundLineAsync(supplierRefundLineCode, warehouse);
            }, "GetSupplierRefundLine", new SoapParameter("SupplierRefundLineCode", supplierRefundLineCode), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<byte[]> GetSupplierRefundDocumentAsync(string supplierRefundLineCode, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetSupplierRefundDocumentAsync(supplierRefundLineCode, warehouse);
                return response.GetSupplierRefundDocumentResult;
            }, "GetSupplierRefundDocument", new SoapParameter("SupplierRefundLineCode", supplierRefundLineCode), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<CustomerBillingLine[]> GetCustomerBillingsLinesAsync(CustomerBillingFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetCustomerBillingsLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("InvoicedAccountCode", f.InvoicedAccountCode), new SoapParameter("ClientSorderCode", f.ClientSorderCode), new SoapParameter("DeliveryNumber", f.DeliveryNumber), new SoapParameter("WithDetail", f.ShowDetails));
        }

        public static async Task<CustomerBillingLine[]> GetCustomerBillingLinesAsync(string customerBillingId, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetCustomerBillingLinesAsync(customerBillingId, warehouse);
            }, "GetCustomerBillingLines", new SoapParameter("CustomerBillingId", customerBillingId), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<byte[]> GetCustomerBillingDocumentAsync(string customerBillingId, string warehouse)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetCustomerBillingDocumentAsync(customerBillingId, warehouse);
                return response.GetCustomerBillingDocumentResult;
            }, "GetCustomerBillingDocument", new SoapParameter("CustomerBillingId", customerBillingId), new SoapParameter("Warehouse", warehouse));
        }

        public static async Task<InvoiceWaitingLine[]> GetInvoiceWaitingLinesAsync(ExpeditionFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetInvoiceWaitingLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("ClientSorderCode", f.ClientSorderCode), new SoapParameter("DeliveryType", f.DeliveryType), new SoapParameter("ContainerNo", f.ContainerNo), new SoapParameter("DeliveryNumber", f.DeliveryNumber));
        }


        public static async Task<RefundWatingLine[]> GetRefundWaitingLinesAsync(ReturnFilter f)
        {
            return await CallServiceAsync(async () =>
            {
                var response = await Client.GetRefundWaitingLinesAsync(
                    f.FirstDate,
                    f.LastDate,
                    f.AccountCode,
                    f.ProductCode,
                    f.ReturnCode,
                    f.ReturnClientCode);

                return response.GetRefundWaitingLinesResult;
            }, "GetRefundWaitingLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ProductCode", f.ProductCode), new SoapParameter("ReturnCode", f.ReturnCode), new SoapParameter("ReturnClientCode", f.ReturnClientCode));
        }

        public static async Task<ShippingCostLine[]> GetShippingCostLinesAsync(AGRA_EASY_MOBILE.Models.ShippingCostFilter f)
        {
            return await CallServiceAsync(async () =>
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
            }, "GetShippingCostLines", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("Warehouse", f.Warehouse), new SoapParameter("AccountCode", f.AccountCode), new SoapParameter("ShippingCostDeliveryNumber", f.ShippingCostDeliveryNumber), new SoapParameter("TurnoverDeliveryNumber", f.TurnoverDeliveryNumber), new SoapParameter("WithDetail", f.ShowDetails), new SoapParameter("WithBilledLines", f.WithBilledLines), new SoapParameter("WithExemptLines", f.WithExemptLines), new SoapParameter("WithWaitingLines", f.WithWaitingLines));
        }



        public static async Task<ShippingWarning[]> GetShippingWarningListAsync(AGRA_EASY_MOBILE.Models.ShippingWarningFilter f, decimal? offset, decimal? count)
        {
            var accountCode = IsClient
                ? CurrentAccount?.AccountCode
                : f.AccountCode;

            return await CallServiceAsync(async () =>
            {
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
            }, "GetShippingWarningList", new SoapParameter("FirstDate", f.FirstDate), new SoapParameter("LastDate", f.LastDate), new SoapParameter("AccountCode", accountCode ?? string.Empty), new SoapParameter("ContainerNo", f.ContainerNo ?? string.Empty), new SoapParameter("Warehouse", f.Warehouse ?? string.Empty), new SoapParameter("OnlyShortMessage", true), new SoapParameter("Offset", offset), new SoapParameter("Count", count));
        }

        public static async Task<ShippingWarning[]> GetShippingWarningAsync(string shippingWarningId, string originalWarehouse)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetShippingWarningAsync(shippingWarningId, originalWarehouse);
            }, "GetShippingWarning", new SoapParameter("ShippingWarningId", shippingWarningId), new SoapParameter("OriginalWarehouse", originalWarehouse));
        }


        public static async Task<bool> IsReturnSystemsManagerAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsReturnSystemsManagerAsync();
            }, "IsReturnSystemsManager");
        }

        public static async Task<bool> IsReturnSystemsSuperManagerAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsReturnSystemsSuperManagerAsync();
            }, "IsReturnSystemsSuperManager");
        }

        public static async Task<ClientAccount> SetReturnBasketAccountCodeAsync(string accountCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.SetReturnBasketAccountCodeAsync(accountCode);
            }, "SetReturnBasketAccountCode", new SoapParameter("AccountCode", accountCode));
        }

        public static async Task<ReturnableArticle[]> FindReturnableArticleListAsync(string keyword)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.FindReturnableArticleListAsync(keyword);
            }, "FindReturnableArticleList", new SoapParameter("Keyword", keyword));
        }

        public static async Task<ReturnableArticle[]> FindProductCodeListAsync(string keyword, bool onlyActiveProduct, bool isGenCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.FindProductCodeListAsync(keyword, onlyActiveProduct, isGenCode);
            }, "FindProductCodeList", new SoapParameter("Keyword", keyword), new SoapParameter("OnlyActiveProduct", onlyActiveProduct), new SoapParameter("IsGenCode", isGenCode));
        }

        public static async Task<DeliveryLinesStatus[]> GetArticlesReturnableDeliveryLinesAsync(string productCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetArticlesReturnableDeliveryLinesAsync(productCode);
            }, "GetArticlesReturnableDeliveryLines", new SoapParameter("ProductCode", productCode));
        }

        public static async Task<string> GetReturnBasketAccountCodeAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetReturnBasketAccountCodeAsync();
            }, "GetReturnBasketAccountCode");
        }

        public static async Task<string> GetDepotSupplierCodeAsync(string accountCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetDepotSupplierCodeAsync(accountCode);
            }, "GetDepotSupplierCode", new SoapParameter("AccountCode", accountCode));
        }

        public static async Task<Warehouse[]> GetWarehousesListV2Async()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetWarehousesListV2Async();
            }, "GetWarehousesListV2");
        }

        public static async Task AddReturnBasketLineAsync(string warehouse, string contretuIndex, decimal quantity, bool isGarantie, string motif, string originalProduct)
        {
            await CallServiceAsync(async () =>
            {
                await Client.AddReturnBasketLineAsync(warehouse, contretuIndex, quantity, isGarantie, motif, originalProduct);
                return true;
            }, "AddReturnBasketLine", new SoapParameter("Warehouse", warehouse), new SoapParameter("ContretuIndex", contretuIndex), new SoapParameter("Quantity", quantity), new SoapParameter("IsGarantie", isGarantie), new SoapParameter("Motif", motif), new SoapParameter("OriginalProduct", originalProduct));
        }

        public static async Task<ReturnBasketLine[]> GetReturnBasketLinesAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetReturnBasketLinesAsync();
            }, "GetReturnBasketLines");
        }

        public static async Task DeleteReturnBasketLineAsync(string returnNoticeLineId)
        {
            await CallServiceAsync(async () =>
            {
                await Client.DeleteReturnBasketLineAsync(returnNoticeLineId);
                return true;
            }, "DeleteReturnBasketLine", new SoapParameter("ReturnNoticeLineId", returnNoticeLineId));
        }

        public static async Task<ReturnNoticeKey> ReturnBasketCheckOutAsync(string returnClientCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.ReturnBasketCheckOutAsync(returnClientCode);
            }, "ReturnBasketCheckOut", new SoapParameter("ReturnClientCode", returnClientCode));
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
            return await CallServiceAsync(async () =>
            {
                return await Client.GetOrderBasketAccountCodeAsync();
            }, "GetOrderBasketAccountCode");
        }

        public static async Task<ClientAccount> SetOrderBasketAccountCodeAsync(string accountCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.SetOrderBasketAccountCodeAsync(accountCode);
            }, "SetOrderBasketAccountCode", new SoapParameter("AccountCode", accountCode));
        }

        public static async Task<ClientAccount[]> FindClientAccountAsync(string keyword)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.FindClientAccountAsync(keyword);
            }, "FindClientAccount", new SoapParameter("Keyword", keyword));
        }

        public static async Task<bool> IsSalesConditionManagerAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsSalesConditionManagerAsync();
            }, "IsSalesConditionManager");
        }

        public static async Task<bool> IsClientOrderManagerAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsClientOrderManagerAsync();
            }, "IsClientOrderManager");
        }

        public static async Task<bool> IsOrderManagerAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsOrderManagerAsync();
            }, "IsOrderManager");
        }

        public static async Task<bool> IsPreOrderManagerAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsPreOrderManagerAsync();
            }, "IsPreOrderManager");
        }

        public static async Task<bool> IsCatalogManagerAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsCatalogManagerAsync();
            }, "IsCatalogManager");
        }

        public static async Task<bool> AllowChangeDeliveryAddressAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.AllowChangeDeliveryAddressAsync();
            }, "AllowChangeDeliveryAddress");
        }

        public static async Task<bool> IsExpressSystemBlockedAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsExpressSystemBlockedAsync();
            }, "IsExpressSystemBlocked");
        }

        public static async Task<bool> IsMagasinSystemBlockedAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsMagasinSystemBlockedAsync();
            }, "IsMagasinSystemBlocked");
        }

        public static async Task<CatalogArticle[]> FindArticleListAsync(string filter, bool showAll)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.FindArticleListAsync(filter, showAll);
            }, "FindArticleList", new SoapParameter("Filter", filter), new SoapParameter("ShowAll", showAll));
        }

        public static async Task<Vehicule> GetVehiculeFromImmatriculationAsync(string immatriculation)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetVehiculeFromImmatriculationAsync(immatriculation);
            }, "GetVehiculeFromImmatriculation", new SoapParameter("Immatriculation", immatriculation));
        }

        public static async Task<VehicleSearchResult> GetKTypeAlternativeAsync(string kTypNo, string treeNodeId)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetKTypeAlternativeAsync(kTypNo, treeNodeId);
            }, "GetKTypeAlternative", new SoapParameter("KTypNo", kTypNo), new SoapParameter("TreeNodeId", treeNodeId));
        }

        public static async Task<ArticlePriceAndStock[]> GetAllConditionForArticleAndAlternativeAsync(string shortProductCode, string supplierCode, string catalog, bool showAll)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetAllConditionForArticleAndAlternativeAsync(shortProductCode, supplierCode, catalog, showAll);
            }, "GetAllConditionForArticleAndAlternative", new SoapParameter("ShortProductCode", shortProductCode), new SoapParameter("SupplierCode", supplierCode), new SoapParameter("Catalog", catalog), new SoapParameter("ShowAll", showAll));
        }

        public static async Task<ArticlFile[]> GetArticleFileListAsync(string brandNo, string articleNo)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetArticleFileListAsync(brandNo, articleNo);
            }, "GetArticleFileList", new SoapParameter("BrandNo", brandNo), new SoapParameter("ArticleNo", articleNo));
        }

        public static async Task<ArticleReference[]> GetArticleReferenceListAsync(string brandNo, string articleNo)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetArticleReferenceListAsync(brandNo, articleNo);
            }, "GetArticleReferenceList", new SoapParameter("BrandNo", brandNo), new SoapParameter("ArticleNo", articleNo));
        }

        public static async Task<ArticlVehicle[]> GetArticleVehicleListAsync(string brandNo, string articleNo)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetArticleVehicleListAsync(brandNo, articleNo);
            }, "GetArticleVehicleList", new SoapParameter("BrandNo", brandNo), new SoapParameter("ArticleNo", articleNo));
        }

        public static async Task<ArticleAttribute[]> GetArticleAttributeListAsync(string brandNo, string articleNo)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetArticleAttributeListAsync(brandNo, articleNo);
            }, "GetArticleAttributeList", new SoapParameter("BrandNo", brandNo), new SoapParameter("ArticleNo", articleNo));
        }

        public static async Task<ArticlePriceAndStock[]> GetLocalConditionForArticleListAsync(GenericArticle[] articleList)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetLocalConditionForArticleListAsync(articleList);
            }, "GetLocalConditionForArticleList", new SoapParameter("ArticleList", articleList));
        }

        public static async Task<ArticlePriceAndStock> GetAllConditionForArticleAsync(string shortProductCode, string supplierCode, string catalog, int quantitative)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetAllConditionForArticleAsync(shortProductCode, supplierCode, catalog, quantitative);
            }, "GetAllConditionForArticle", new SoapParameter("ShortProductCode", shortProductCode), new SoapParameter("SupplierCode", supplierCode), new SoapParameter("Catalog", catalog), new SoapParameter("Quantitative", quantitative));
        }

        public static async Task<ArticlePriceAndStock> GetLocalConditionForArticleAsync(string shortProductCode, string supplierCode, string catalog, int quantitative)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetLocalConditionForArticleAsync(shortProductCode, supplierCode, catalog, quantitative);
            }, "GetLocalConditionForArticle", new SoapParameter("ShortProductCode", shortProductCode), new SoapParameter("SupplierCode", supplierCode), new SoapParameter("Catalog", catalog), new SoapParameter("Quantitative", quantitative));
        }

        public static async Task AddArticleListToWarehouseShoppingCartAsync(OrderLine[] orderLines)
        {
            await CallServiceAsync(async () =>
            {
                await Client.AddArticleListToWarehouseShoppingCartAsync(orderLines);
                return true;
            }, "AddArticleListToWarehouseShoppingCart", new SoapParameter("ShoppingCartLineList", orderLines));
        }


        public static async Task<int> GetExternalStockStatusAsync(string productCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetExternalStockStatusAsync(productCode);
            }, "GetExternalStockStatus", new SoapParameter("ProductCode", productCode));
        }

        public static async Task<ExternalSorderReturnBack> SendExternalCommandAsync(string productCode, int quantity, string clientSorderNumber)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.SendExternalCommandAsync(productCode, quantity, clientSorderNumber);
            }, "SendExternalCommand", new SoapParameter("ProductCode", productCode), new SoapParameter("Quantity", quantity), new SoapParameter("ClientSorderNumber", clientSorderNumber));
        }

        public static async Task<SorderBasketLine[]> GetOrderBasketLinesAsync(bool withConsigneAndEcotaxe, bool onlyLocalDisponibility, string warehouseCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetOrderBasketLinesAsync(withConsigneAndEcotaxe, onlyLocalDisponibility, warehouseCode);
            }, "GetOrderBasketLines", new SoapParameter("WithConsigneAndEcotaxe", withConsigneAndEcotaxe), new SoapParameter("OnlyLocalDisponibility", onlyLocalDisponibility), new SoapParameter("WarehouseCode", warehouseCode));
        }

        public static async Task<ShoppingCartWarehouseInformation[]> GetShoppingCartDashboardAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetShoppingCartDashboardAsync();
            }, "GetShoppingCartDashboard");
        }

        public static async Task<ShoppingCartWarehouseInformation[]> GetWarehouseDashboardAsync(bool forAllPlatform)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetWarehouseDashboardAsync(forAllPlatform);
            }, "GetWarehouseDashboard", new SoapParameter("ForAllPlatform", forAllPlatform));
        }

        public static async Task<int> GetShoppingCartUnitCountAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetShoppingCartUnitCountAsync();
            }, "GetShoppingCartUnitCount");
        }

        public static async Task<decimal> GetTotalSorderCoastAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetTotalSorderCoastAsync();
            }, "GetTotalSorderCoast");
        }

        public static async Task<decimal> GetGarageTotalSorderCoastAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetGarageTotalSorderCoastAsync();
            }, "GetGarageTotalSorderCoast");
        }

        public static async Task DeleteShoppingCartLineAsync(string shoppingCartLine)
        {
            await CallServiceAsync(async () =>
            {
                await Client.DeleteShoppingCartLineAsync(shoppingCartLine);
                return true;
            }, "DeleteShoppingCartLine", new SoapParameter("ShoppingCartLine", shoppingCartLine));
        }

        public static async Task UpdateShoppingCartLineQuantityAsync(string productCode, string warehouseCode, string newQuantity)
        {
            await CallServiceAsync(async () =>
            {
                await Client.UpdateShoppingCartLineQuantityAsync(productCode, warehouseCode, newQuantity);
                return true;
            }, "UpdateShoppingCartLineQuantity", new SoapParameter("ProductCode", productCode), new SoapParameter("WrehouseCode", warehouseCode), new SoapParameter("NewQuantity", newQuantity));
        }

        public static async Task<bool> IsPromotionActiveAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.IsPromotionActiveAsync();
            }, "IsPromotionActive");
        }

        public static async Task<PromotionBonusLine[]> TreatePromotionWithPromotionalCodeAsync(string promotionalCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.TreatePromotionWithPromotionalCodeAsync(promotionalCode);
            }, "TreatePromotionWithPromotionalCode", new SoapParameter("PromotionalCode", promotionalCode));
        }

        public static async Task ApplicatePromotionAsync(string productCode)
        {
            await CallServiceAsync(async () =>
            {
                await Client.ApplicatePromotionAsync(productCode);
                return true;
            }, "ApplicatePromotion", new SoapParameter("ProductCode", productCode));
        }

        public static async Task CancelPromotionAsync()
        {
            await CallServiceAsync(async () =>
            {
                await Client.CancelPromotionAsync();
                return true;
            }, "CancelPromotion");
        }

        public static async Task ClearShoppingCartAsync()
        {
            await CallServiceAsync(async () =>
            {
                await Client.ClearShoppingCartAsync();
                return true;
            }, "ClearShoppingCart");
        }

        public static async Task<ShoppingCartResponse[]> WarehouseShoppingCartCheckOutAsync(string type, string sorderNumber)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.WarehouseShoppingCartCheckOutAsync(type, sorderNumber);
            }, "WarehouseShoppingCartCheckOut", new SoapParameter("Type", type), new SoapParameter("SorderNumber", sorderNumber));
        }

        public static async Task<ShoppingCartResponse[]> SelectedWarehouseShoppingCartCheckOutAsync(string type, string sorderNumber, string warehouseCode)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.SelectedWarehouseShoppingCartCheckOutAsync(type, sorderNumber, warehouseCode);
            }, "SelectedWarehouseShoppingCartCheckOut", new SoapParameter("Type", type), new SoapParameter("SorderNumber", sorderNumber), new SoapParameter("WarehouseCode", warehouseCode));
        }

        public static async Task<DelivryAddress> GetDelivryAddressAsync()
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.GetDelivryAddressAsync();
            }, "GetDelivryAddress");
        }

        public static async Task SetDelivryAddressAsync(string line1, string line2, string zip, string city, string country, string contactName, string contactTel, string contactFax, string contactMail)
        {
            await CallServiceAsync(async () =>
            {
                await Client.SetDelivryAddressAsync(line1, line2, zip, city, country, contactName, contactTel, contactFax, contactMail);
                return true;
            }, "SetDelivryAddress", new SoapParameter("Address_line1", line1), new SoapParameter("Address_line2", line2), new SoapParameter("Zip", zip), new SoapParameter("City", city), new SoapParameter("Country", country), new SoapParameter("ContactName", contactName), new SoapParameter("ContactTel", contactTel), new SoapParameter("ContactFax", contactFax), new SoapParameter("ContactMail", contactMail));
        }

        public static async Task SetDefaultDelivryAddressAsync()
        {
            await CallServiceAsync(async () =>
            {
                await Client.SetDefaultDelivryAddressAsync();
                return true;
            }, "SetDefaultDelivryAddress");
        }

        public static async Task SetDelivryDateAsync(DateTime delivryDate)
        {
            await CallServiceAsync(async () =>
            {
                await Client.SetDelivryDateAsync(delivryDate);
                return true;
            }, "SetDelivryDate", new SoapParameter("DelivryDate", delivryDate));
        }

        public static async Task ClearDelivryDateAsync()
        {
            await CallServiceAsync(async () =>
            {
                await Client.ClearDelivryDateAsync();
                return true;
            }, "ClearDelivryDate");
        }

        public static async Task SetLotsNumberAsync(string lotsNumber)
        {
            await CallServiceAsync(async () =>
            {
                await Client.SetLotsNumberAsync(lotsNumber);
                return true;
            }, "SetLotsNumber", new SoapParameter("LotsNumber", lotsNumber));
        }

        public static async Task ClearLotsNumberAsync()
        {
            await CallServiceAsync(async () =>
            {
                await Client.ClearLotsNumberAsync();
                return true;
            }, "ClearLotsNumber");
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
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                return false;

            var previousAccount = CurrentAccount;
            string previousLogin = CurrentLogin;

            if (Client != null && (IsConnected || CurrentAccount != null))
                await CloseCurrentClientBeforeNewConnectionAsync();

            CurrentUrl = await PlatformEndpointResolver.GetServiceUrlAsync(warehouse, useSecondary);

            var binding = CurrentUrl.ToLower().StartsWith("https")
                ? new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                : new BasicHttpBinding(BasicHttpSecurityMode.None);

            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.AllowCookies = true;

                if (UseManualSoap)
                {
                    ManualClient?.Dispose();
                    ManualClient = new EasySoapManualClient(CurrentUrl);
                    CurrentAccount = await ManualClient.InvokeAsync<global::Services.EasyAccount>(
                        "Connexion",
                        new SoapParameter("UserName", user),
                        new SoapParameter("Password", pass),
                        new SoapParameter("SoftwareName", "MAUI_AGRA_V10"));
                }
                else
                {
                    Client = new ShoppingCartControllerSoapClient(binding, new EndpointAddress(CurrentUrl));
                    CurrentAccount = await Client.ConnexionAsync(user, pass, "MAUI_AGRA_V10");
                }

                IsConnected = CurrentAccount != null;
                if (IsConnected)
                {
                    CurrentLogin = user;
                    try
                    {
                        IsCustomerBillingManager = UseManualSoap
                            ? await CallSoapAsync<bool>("IsCustomerBillingManager")
                            : await Client.IsCustomerBillingManagerAsync();
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

                if (IsConnected)
                {
                    _ = FirebasePushNotificationService.EnsureRegisteredAsync();
                    _ = MobileNotificationRegistrationService.TryRegisterCurrentDeviceAsync();
                }

            return IsConnected;
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
                ManualClient?.Dispose();
                ManualClient = null;
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
                ManualClient?.Dispose();
                ManualClient = null;
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
                if ((!UseManualSoap && Client == null) || (UseManualSoap && ManualClient == null))
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

            if ((!UseManualSoap && Client == null) || (UseManualSoap && ManualClient == null))
                return false;

            try
            {
                sessionStillConnected = UseManualSoap
                    ? await CallSoapAsync<bool>("IsConnected")
                    : await Client.IsConnectedAsync();
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

        public static async Task<string?> RegisterMobileNotificationDeviceAsync(string registrationXml)
        {
            return await CallServiceAsync(async () =>
            {
                return await Client.RegisterMobileNotificationDeviceAsync(registrationXml);
            }, "RegisterMobileNotificationDevice", new SoapParameter("registrationXml", registrationXml));
        }

        public static async Task<ShippingWarning[]> GetShippingWarningFromNotificationAsync(string shippingWarningId, string originalWarehouse)
        {
            var targetUrl = await PlatformEndpointResolver.GetRequiredPrimaryServiceUrlAsync(originalWarehouse);

            if (UseManualSoap)
            {
                using var manualClient = new EasySoapManualClient(targetUrl);
                return await manualClient.InvokeAsync<ShippingWarning[]>(
                    "GetShippingWarning",
                    new SoapParameter("ShippingWarningId", shippingWarningId),
                    new SoapParameter("OriginalWarehouse", originalWarehouse)) ?? Array.Empty<ShippingWarning>();
            }

            var binding = targetUrl.ToLowerInvariant().StartsWith("https")
                ? new BasicHttpBinding(BasicHttpSecurityMode.Transport)
                : new BasicHttpBinding(BasicHttpSecurityMode.None);

            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.AllowCookies = true;

            var temporaryClient = new ShoppingCartControllerSoapClient(binding, new EndpointAddress(targetUrl));
            try
            {
                return await temporaryClient.GetShippingWarningAsync(shippingWarningId, originalWarehouse)
                    ?? Array.Empty<ShippingWarning>();
            }
            finally
            {
                try
                {
                    await temporaryClient.CloseAsync();
                }
                catch
                {
                    temporaryClient.Abort();
                }
            }
        }

        private static Task<T> CallServiceAsync<T>(Func<Task<T>> generatedCall, string operation, params SoapParameter[] parameters)
        {
            if (UseManualSoap)
                return ExecuteWithRetryAsync(() => CallSoapAsync<T>(operation, parameters));

            return ExecuteWithRetryAsync(generatedCall);
        }

        private static async Task CallServiceVoidAsync(Func<Task> generatedCall, string operation, params SoapParameter[] parameters)
        {
            if (UseManualSoap)
            {
                await ExecuteWithRetryAsync(async () =>
                {
                    await CallSoapVoidAsync(operation, parameters);
                    return true;
                });
                return;
            }

            await ExecuteWithRetryAsync(async () =>
            {
                await generatedCall();
                return true;
            });
        }

        private static async Task<T> CallSoapAsync<T>(string operation, params SoapParameter[] parameters)
        {
            if (ManualClient == null)
                throw new InvalidOperationException("Le client SOAP manuel n'est pas initialise.");

            var result = await ManualClient.InvokeAsync<T>(operation, parameters);
            return result!;
        }

        private static async Task CallSoapVoidAsync(string operation, params SoapParameter[] parameters)
        {
            if (ManualClient == null)
                throw new InvalidOperationException("Le client SOAP manuel n'est pas initialise.");

            await ManualClient.InvokeVoidAsync(operation, parameters);
        }
    }
}
