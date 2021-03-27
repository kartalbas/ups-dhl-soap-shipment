using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ShipmentDHL;
using ShipmentLib;

namespace ShipmentTests
{
    [TestClass]
    public class ShipmentDHL_Tests
    {

        private Shipment CreateReceiver()
        {
            Shipment objShipment = new Shipment();
            objShipment.ExecutedByShipmentTests = true;
            //objShipment.SignerTitle = "Geschäftsführer";
            objShipment.ReceiverCompanyName = "simetrix GmbH";
            objShipment.ReceiverFirstName = "Mehmet Kartalbas";
            objShipment.ReceiverContactEMail = "mk@simetrix.ch";
            objShipment.ReceiverContactName = "Mehmet Kartalbas";
            objShipment.ReceiverContactPhone = "+41793449399";
            objShipment.ReceiverStreentNr = "-";
            objShipment.InvoiceNumber = "1234567";
            objShipment.Packages.Add(new Shipment.Package());
            objShipment.IncoTerm = objShipment.IncoTerm_CIP_Frachtfrei_Versichert;
            return objShipment;
        }

        private Shipment CreateExportDocument(Shipment objShipment)
        {
            objShipment.ExportDocumentInvoiceNumber = "12312341231".MaxLength(30);
            objShipment.ExportDocumentInvoiceDate = DateTime.Today.ToString(objShipment.SDF).MaxLength(10);
            objShipment.ExportDocumentCommodityCode = "".MaxLength(30);
            objShipment.ExportDocumentTermsOfTrade = objShipment.IncoTerm.MaxLength(3);
            objShipment.ExportDocumentAmount = "1".MaxLength(22);
            objShipment.ExportDocumentDescription = "Commodity";
            objShipment.ExportDocumentCountryCodeOrigin = SettingController.ShipperCountryCode.MaxLength(2);
            objShipment.ExportDocumentAdditionalFee = 0.0M;
            objShipment.ExportDocumentCustomsValue = 299.00M;
            objShipment.ExportDocumentCustomsCurrency = "EUR";
            objShipment.ExportDocumentPermitNumber = "".MaxLength(30);
            objShipment.ExportDocumentAttestationNumber = "".MaxLength(30);
            objShipment.ExportDocumentExportTypeDescription = "".MaxLength(30);
            objShipment.ExportDocumentMRNNumber = "".MaxLength(9);

            objShipment.ItemPositions.Add(new Shipment.ItemPosition()
            {
                Amount = ("1").MaxLength(22),
                CommodityCode = ("12323").MaxLength(30),
                CountryCodeOrigin = SettingController.ShipperCountryCode.MaxLength(3),
                CustomsCurrency = objShipment.ExportDocumentCustomsCurrency.MaxLength(3),
                CustomsValue = 100.00M,
                Description = ("Article Position 1").MaxLength(40),
                GrossWeightInKG = 1.0M,
                NetWeightInKG = 0.9M
            });

            objShipment.ItemPositions.Add(new Shipment.ItemPosition()
            {
                Amount = ("1").MaxLength(22),
                CommodityCode = ("3434534").MaxLength(30),
                CountryCodeOrigin = SettingController.ShipperCountryCode.MaxLength(3),
                CustomsCurrency = objShipment.ExportDocumentCustomsCurrency.MaxLength(3),
                CustomsValue = 199.00M,
                Description = ("Article Position 2").MaxLength(40),
                GrossWeightInKG = 1.0M,
                NetWeightInKG = 0.9M
            });

            objShipment.ItemPositions.Add(new Shipment.ItemPosition()
            {
                Amount = ("2").MaxLength(22),
                CommodityCode = ("3434534").MaxLength(30),
                CountryCodeOrigin = SettingController.ShipperCountryCode.MaxLength(3),
                CustomsCurrency = objShipment.ExportDocumentCustomsCurrency.MaxLength(3),
                CustomsValue = 9.00M,
                Description = ("Article Position 3").MaxLength(40),
                GrossWeightInKG = 1.0M,
                NetWeightInKG = 0.9M
            });

            return objShipment;
        }

        [TestMethod]
        public void DHL_EPN_National()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "Kuthstrasse";
            objShipment.ReceiverStreentNr = "70";
            objShipment.ReceiverCity = "Köln";
            objShipment.ReceiverZip = "51107";
            objShipment.ReceiverCountryCode = "DE";
            objShipment.DDProdCode = objShipment.DDProdCode_EPN_EUROPACK_NATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_DELETE()
        {
            DHL_EPN_National();

            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "Kuthstrasse";
            objShipment.ReceiverStreentNr = "70";
            objShipment.ReceiverCity = "Köln";
            objShipment.ReceiverZip = "51107";
            objShipment.ReceiverCountryCode = "DE";
            objShipment.DDProdCode = objShipment.DDProdCode_EPN_EUROPACK_NATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));

            objRequest.DeleteOldShipmentDD();

        }

        [TestMethod]
        public void DHL_EPN_National_With_COD()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "Grabenwiese 5";
            objShipment.ReceiverCity = "Neu-Ulm";
            objShipment.ReceiverZip = "89231";
            objShipment.ReceiverCountryCode = "DE";
            objShipment.CODAmount = 199.99M;
            objShipment.CODCurrency = "EUR";
            objShipment.DDProdCode = objShipment.DDProdCode_EPN_EUROPACK_NATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_EPI_International_EU_GB()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "62 Frith St";
            objShipment.ReceiverCity = "London";
            objShipment.ReceiverZip = "W1D 3JN";
            objShipment.ReceiverCountryCode = "GB";
            objShipment.DDProdCode = objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_EPI_International_EU_OTHER()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "44 Meent";
            objShipment.ReceiverCity = "Rotterdam";
            objShipment.ReceiverZip = "3011JL";
            objShipment.ReceiverCountryCode = "NL";
            objShipment.DDProdCode = objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_EPI_International_OTHER()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateExportDocument(CreateReceiver());
            objShipment.IncoTerm = objShipment.IncoTerm_DDP_Geliefert_Verzollt;
            objShipment.ExportDocumentTermsOfTrade = objShipment.IncoTerm;

            objShipment.ReceiverStreet = "Via Ginellas 3";
            objShipment.ReceiverCity = "Bonaduz";
            objShipment.ReceiverZip = "7402";
            objShipment.ReceiverCountryCode = "CH";
            objShipment.DDProdCode = objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_EPI_International_ISLAND()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateExportDocument(CreateReceiver());
            objShipment.IncoTerm = objShipment.IncoTerm_DDP_Geliefert_Verzollt;
            objShipment.ExportDocumentTermsOfTrade = objShipment.IncoTerm;

            objShipment.ReceiverStreet = "Hlidarsmara 2";
            objShipment.ReceiverCity = "Kopavogur";
            objShipment.ReceiverZip = "201";
            objShipment.ReceiverCountryCode = "IS";
            objShipment.DDProdCode = objShipment.DDProdCode_EPI_EUROPAK_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_BPI_International_EU_GB()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "62 Frith St";
            objShipment.ReceiverCity = "London";
            objShipment.ReceiverZip = "W1D 3JN";
            objShipment.ReceiverCountryCode = "GB";
            objShipment.DDProdCode = objShipment.DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_BPI_International_EU_GB_With_COD()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "62 Frith St";
            objShipment.ReceiverCity = "London";
            objShipment.ReceiverZip = "W1D 3JN";
            objShipment.ReceiverCountryCode = "GB";
            objShipment.CODAmount = 199.99M;
            objShipment.CODCurrency = "EUR";
            objShipment.DDProdCode = objShipment.DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_BPI_International_EU_OTHER()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateReceiver();

            objShipment.ReceiverStreet = "44 Meent";
            objShipment.ReceiverCity = "Rotterdam";
            objShipment.ReceiverZip = "3011JL";
            objShipment.ReceiverCountryCode = "NL";
            objShipment.DDProdCode = objShipment.DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }

        [TestMethod]
        public void DHL_BPI_International_EU_OTHER_With_COD()
        {
            Logger.Instance.Log(TraceEventType.Information, 0, "Unit Test: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Shipment objShipment = CreateExportDocument(CreateReceiver());

            objShipment.ReceiverStreet = "44 Meent";
            objShipment.ReceiverCity = "Rotterdam";
            objShipment.ReceiverZip = "3011JL";
            objShipment.ReceiverCountryCode = "NL";
            objShipment.CODAmount = 199.99M;
            objShipment.CODCurrency = "EUR";
            objShipment.DDProdCode = objShipment.DDProdCode_BPI_BUSINESS_PAKET_INTERNATIONAL;

            ShipmentRequestBuilderOld objRequest = new ShipmentRequestBuilderOld(objShipment);
            var objResult = objRequest.CreateNewShipmentDDRequest();

            string strDhlException = string.Empty;
            if (objResult.CreationState[0].StatusMessage.Length > 1)
            {
                foreach (string strLine in objResult.CreationState[0].StatusMessage)
                {
                    strDhlException += string.Format("{0}\n", strLine);
                }
            }

            Assert.IsNotNull(objResult, "FAIL - objResult is NULL!");
            Assert.IsTrue(objResult.status.StatusCode == "0", string.Format("FAIL - Status is: {0} - {1} \n {2}", objResult.status.StatusCode.ToString(), objResult.status.StatusMessage, strDhlException));
        }
    }
}
