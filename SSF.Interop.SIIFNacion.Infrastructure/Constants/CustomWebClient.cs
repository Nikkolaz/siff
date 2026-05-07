using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSF.Interop.SIIFNacion.Infrastructure.Constants
{
    public static class CustomWebClient
    {
        public const string controllerIhiLogs = "IHILogsApis/";
        public const string controllerIhiGeneralSettingApi = "Apis/";
        public const string controllerIhiAudit = "IhiAudit/";
        public const string controllerIhiCustomerSettingsApi = "Setting/";
        public const string controllerIhiPrinter = "Printer/";
        public const string controllerIhiIfiProduct = "IFIProduct/";
        public const string controllerIhiCards = "Cards/";
        public const string controllerIhiPrinters = "Printers/";
        public const string controllerIhiClient = "Clients/";
        public const string controllerIhiPc = "Pcs/";

        public const string controllerIfiValidator = "IfiValidator/";
        public const string controllerIhiAuth = "Auth/";
        public const string controllerIhiBaseStatus = "Statuses/";
        public const string controllerIhiBaseDataSource = "DataSource/";
        public const string controllerIhiBaseFunctions = "Functions/";
        public const string controllerIhiBaseIssuanceMode = "IssuanceMode/";
        public const string controllerIhiBaseIssuanceType = "IssuanceType/";
        public const string ItemFunction = "ItemFunction/";
        public const string controllerIhiBaseDefinitions = "Definitions/";
        public const string DataHandler = "DataHandler/";
        public const string controllerIhiCustomerPrinter = "Printer/";
        public const string IFIProduct = "IFIProduct/";
        public const string controllerIhiBaseDataItems = "DataItems/";
        public const string controllerIhiBaseConversionMessages = "ConversionMessages/";
        public const string controllerIhiBaseEvents = "Events/";
        public const string controllerIhiBaseNotificationsEvents = "NotificationsEvents/";
        public const string controllerIhiBaseNotificationsTypes = "NotificationsTypes/";
        public const string controllerNotification = "Notification/";
        public const string controllerCmsConfig = "CMSConfig/";

        public const string ByClientId = "ByClientId/";
        public const string methodCreateRow = "CreateRow/";
        public const string methodGetRowByName = "GetRowByName/";
        public const string methodPrinterByName = "PrinterByName";
        public const string methodGetProductByName = "IFIProductByProductName";
        public const string methodGetAllProducts = "AllProducts";
        public const string methodGetCardFormatIdByName = "CardFormatIdByName";
        public const string methodGetPrinterStatus = "PrinterStatus";
        public const string methodGetRow = "GetRow/";
        public const string methodGetPc = "Pc";
        public const string methodCardFormatPrinterByPC = "CardFormatPrinterByPC";

        public const string methodIfiValidator = "Validate";
        public const string methodReservePrinter = "ReservePrinter";
        public const string methodMakePrinterAvailable = "MakePrinterAvailable";
        public const string methodGetToken = "GetToken";
        public const string methodGetAllStatus = "Status";
        public const string methodGetAll = "GetAll/";
        public const string methodGetAllPrinters = "PrinterByClient?clientId=";
        public const string Productsbyclient = "IFIProductsByClient?clientId=";
        public const string methodCreateOrUpdateSettings = "UpdateValueSettings";

        public const string methodExternalAuthConfig = "ExternalAuthConfig/";
        public const string methodGetByClient = "GetByClient";
        public const string methodCmsConfigGetByClient = "GetByClient";
        public const string methodCmsConfigGetById = "GetById";

        public const string methodExternalAuthConfigByClientId = "GetByClient";

    }
}
