using AGRA_EASY_MOBILE.Models;

namespace AGRA_EASY_MOBILE.Models
{
    public static class GlobalState
    {
        private static ExpeditionFilter _currentExpeditionFilter;
        public static ExpeditionFilter CurrentExpeditionFilter
        {
            get => _currentExpeditionFilter ??= new ExpeditionFilter
            {
                FirstDate = DateTime.Now.AddDays(-1),
                LastDate = DateTime.Now
            };
            set => _currentExpeditionFilter = value;
        }

        public static ExpeditionFilter PeekExpeditionFilter() => _currentExpeditionFilter;

        public static void ClearExpeditionFilter()
        {
            _currentExpeditionFilter = null;
        }

        public static bool HasValidAdminExpeditionFilter()
        {
            var filter = _currentExpeditionFilter;
            return filter != null
                && (!string.IsNullOrWhiteSpace(filter.ProductCode)
                    || !string.IsNullOrWhiteSpace(filter.AccountCode)
                    || !string.IsNullOrWhiteSpace(filter.ClientSorderCode)
                    || !string.IsNullOrWhiteSpace(filter.ContainerNo)
                    || !string.IsNullOrWhiteSpace(filter.DeliveryNumber));
        }

        public static bool HasValidAdminInvoiceWaitingFilter()
        {
            var filter = _currentExpeditionFilter;
            return filter != null
                && (!string.IsNullOrWhiteSpace(filter.ProductCode)
                    || !string.IsNullOrWhiteSpace(filter.AccountCode)
                    || !string.IsNullOrWhiteSpace(filter.ClientSorderCode)
                    || !string.IsNullOrWhiteSpace(filter.DeliveryType)
                    || !string.IsNullOrWhiteSpace(filter.ContainerNo)
                    || !string.IsNullOrWhiteSpace(filter.DeliveryNumber));
        }

        public static ExpeditionFilter EnsureClientExpeditionFilter(string accountCode)
        {
            var filter = CurrentExpeditionFilter;
            if (string.IsNullOrWhiteSpace(filter.AccountCode))
                filter.AccountCode = accountCode;
            return filter;
        }

        private static ReturnFilter _currentReturnFilter;
        public static ReturnFilter CurrentReturnFilter
        {
            get => _currentReturnFilter ??= new ReturnFilter
            {
                FirstDate = DateTime.Now.AddDays(-1),
                LastDate = DateTime.Now
            };
            set => _currentReturnFilter = value;
        }

        public static ReturnFilter PeekReturnFilter() => _currentReturnFilter;

        public static void ClearReturnFilter()
        {
            _currentReturnFilter = null;
        }

        public static bool HasValidAdminReturnFilter()
        {
            var filter = _currentReturnFilter;
            return filter != null
                && (!string.IsNullOrWhiteSpace(filter.ProductCode)
                    || !string.IsNullOrWhiteSpace(filter.AccountCode)
                    || !string.IsNullOrWhiteSpace(filter.ReturnCode)
                    || !string.IsNullOrWhiteSpace(filter.ReturnClientCode));
        }

        public static ReturnFilter EnsureClientReturnFilter(string accountCode)
        {
            var filter = CurrentReturnFilter;
            if (string.IsNullOrWhiteSpace(filter.AccountCode))
                filter.AccountCode = accountCode;
            return filter;
        }

        public static ReturnFilter EnsureReturnFilterWithSlidingWeek()
        {
            if (_currentReturnFilter == null)
            {
                _currentReturnFilter = new ReturnFilter
                {
                    FirstDate = DateTime.Now.AddDays(-7),
                    LastDate = DateTime.Now
                };
            }

            return _currentReturnFilter;
        }


        private static CustomerBillingFilter _currentCustomerBillingFilter;
        public static CustomerBillingFilter CurrentCustomerBillingFilter
        {
            get
            {
                if (_currentCustomerBillingFilter == null)
                {
                    var today = DateTime.Today;
                    var currentMonthStart = new DateTime(today.Year, today.Month, 1);
                    _currentCustomerBillingFilter = new CustomerBillingFilter
                    {
                        FirstDate = currentMonthStart.AddMonths(-1),
                        LastDate = today
                    };
                }

                return _currentCustomerBillingFilter;
            }
            set => _currentCustomerBillingFilter = value;
        }

        public static CustomerBillingFilter PeekCustomerBillingFilter() => _currentCustomerBillingFilter;

        public static void ClearCustomerBillingFilter()
        {
            _currentCustomerBillingFilter = null;
        }

        public static bool HasValidAdminCustomerBillingFilter()
        {
            var filter = _currentCustomerBillingFilter;
            return filter != null
                && (!string.IsNullOrWhiteSpace(filter.ProductCode)
                    || !string.IsNullOrWhiteSpace(filter.InvoicedAccountCode)
                    || !string.IsNullOrWhiteSpace(filter.AccountCode)
                    || !string.IsNullOrWhiteSpace(filter.ClientSorderCode)
                    || !string.IsNullOrWhiteSpace(filter.DeliveryNumber));
        }

        public static CustomerBillingFilter EnsureClientCustomerBillingFilter(string accountCode)
        {
            var filter = CurrentCustomerBillingFilter;
            if (string.IsNullOrWhiteSpace(filter.InvoicedAccountCode))
                filter.InvoicedAccountCode = accountCode;
            return filter;
        }

        private static ShippingCostFilter _currentShippingCostFilter;
        public static ShippingCostFilter CurrentShippingCostFilter
        {
            get
            {
                if (_currentShippingCostFilter == null)
                {
                    var defaultRange = GetDefaultShippingCostDateRange();
                    _currentShippingCostFilter = new ShippingCostFilter
                    {
                        FirstDate = defaultRange.FirstDate,
                        LastDate = defaultRange.LastDate,
                        WithWaitingLines = true,
                        WithBilledLines = false,
                        WithExemptLines = false,
                        ShowDetails = false
                    };
                }

                return _currentShippingCostFilter;
            }
            set => _currentShippingCostFilter = value;
        }

        public static (DateTime FirstDate, DateTime LastDate) GetDefaultShippingCostDateRange()
        {
            var today = DateTime.Today;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1);
            var start = today.Day <= 7 ? currentMonthStart.AddMonths(-1) : currentMonthStart;
            var end = currentMonthStart.AddMonths(1).AddDays(-1);
            return (start, end);
        }

        public static ShippingCostFilter PeekShippingCostFilter() => _currentShippingCostFilter;

        public static void ClearShippingCostFilter()
        {
            _currentShippingCostFilter = null;
        }

        public static bool HasValidAdminShippingCostFilter()
        {
            var filter = _currentShippingCostFilter;
            return filter != null && filter.HasRequiredSearchCriterion;
        }

        public static ShippingCostFilter EnsureClientShippingCostFilter(string accountCode)
        {
            var filter = CurrentShippingCostFilter;
            if (string.IsNullOrWhiteSpace(filter.AccountCode)) filter.AccountCode = accountCode;
            return filter;
        }


        private static ShippingWarningFilter _currentShippingWarningFilter;
        public static ShippingWarningFilter CurrentShippingWarningFilter
        {
            get
            {
                if (_currentShippingWarningFilter == null)
                    _currentShippingWarningFilter = new ShippingWarningFilter();

                return _currentShippingWarningFilter;
            }
            set => _currentShippingWarningFilter = value;
        }

        public static ShippingWarningFilter PeekShippingWarningFilter() => _currentShippingWarningFilter;

        public static void ClearShippingWarningFilter()
        {
            _currentShippingWarningFilter = null;
        }

        public static ShippingWarningFilter EnsureClientShippingWarningFilter(string accountCode)
        {
            var filter = CurrentShippingWarningFilter;
            if (string.IsNullOrWhiteSpace(filter.AccountCode))
                filter.AccountCode = accountCode;
            return filter;
        }

    }
}
