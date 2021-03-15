using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DHL_WS_Test.GeschaeftskundenversandWS;

namespace DHL_WS_Test.Geschaeftskundenversand
{
    class Client
    {

        private static String CRED_FILE_PATH = "./Geschaeftskundenversand/Einstellungen/zugangsdaten.ini";
        private static Encoding ENCODING = Encoding.UTF8;
        private static String IS_USERNAME_PROPERTY_NAME = "IntraShip_user";
        private static String IS_PASSWD_PROPERTY_NAME = "IntraShip_password";
        private static String CIG_USERNAME_PROPERTY_NAME = "cig_user";
        private static String CIG_PASSWD_PROPERTY_NAME = "cig_password";


        public static void startTestRequest()
        {
            Dictionary<String, String> credDict = CredentialsFileReader.getSettings(CRED_FILE_PATH, ENCODING);
            String is_username = credDict[IS_USERNAME_PROPERTY_NAME];
            String is_passwd = credDict[IS_PASSWD_PROPERTY_NAME];
            String cig_username;
            String cig_passwd;




            // Service-Stub erstellen
            SWSServicePortTypeClient webService = new SWSServicePortTypeClient("ShipmentServiceSOAP11port0");

            // Endpoint auf Sandbox (https://cig.dhl.de/services/sandbox/soap) umkonfigurieren
            webService.Endpoint.Address = new System.ServiceModel.EndpointAddress("https://cig.dhl.de/services/sandbox/soap");
            // Endpoint auf Http BasicAuth konfigurien
            System.ServiceModel.BasicHttpBinding Binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport);
            Binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;
            // Settings übernehmen
            webService.Endpoint.Binding = Binding;

            // Prompt the user for username & password

            GetPassword(credDict, out cig_username, out cig_passwd);

            // Basic Auth User und Password setzen
            webService.ClientCredentials.UserName.UserName = cig_username;
            webService.ClientCredentials.UserName.Password = cig_passwd;

            AuthentificationType auth = new AuthentificationType();
            auth.user = is_username;
            auth.signature = is_passwd;
            auth.type = "0";

            try
            {
                short input = 0;
                do
                {
                    input = readMainMenuInput();
                    switch (input)
                    {
                        case 1:
                            runCreateShipmentDDRequest(webService, auth);
                            break;
                        case 2:
                            runGetLabelDDRequest(webService, auth);
                            break;
                        case 3:
                            runDeleteShipmentDDRequest(webService, auth);
                            break;
                    }
                    if (input == -1)
                        Console.WriteLine("Bitte, korrigieren Sie Ihre Eingabe!");
                    else if (input != 0)
                    {
                        Console.Write("Bitte, drücken Sie die Eingabe-Taste um fortzufahren.");
                        Console.ReadLine();
                    }
                } while (input != 0);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.ReadLine();
            }

        }


        private static short readMainMenuInput()
        {
            Console.WriteLine("Geschaeftskundenversand Operationen: " + "\n" +
                "1 - runCreateShipmentDDRequest" + "\n" +
                "2 - runGetLabelDDRequest" + "\n" +
                "3 - runDeleteShipmentDDRequest" + "\n" +
                "0 - Programm beenden" + "\n");
            Console.Write("Waehlen Sie die gewünschte Operation: ");
            String choice = Console.ReadLine();
            try
            {
                return short.Parse(choice);
            }
            catch
            {
                return -1;
            }
        }

        private static void runDeleteShipmentTDRequest(SWSServicePortTypeClient port, AuthentificationType auth)
        {

            Console.WriteLine("Sendungsnummer eingeben:");
            String Sendungsnummer = Console.ReadLine();
            DeleteShipmentTDRequest tdRequest = GeschaeftskundenversandRequestBuilder.getDeleteShipmentTDRequest(Sendungsnummer);

            try
            {
                DeleteShipmentResponse delResponse = port.deleteShipmentTD(auth, tdRequest);

                //Response status
                Statusinformation status = delResponse.Status;
                String statusMessage = status.StatusMessage;
                DeletionState[] delStates = delResponse.DeletionState;

                Console.Write("deleteShipmentTDRequest: \n" +
                        "Status-Nachricht: " + statusMessage + "\n");


                foreach (DeletionState delState in delStates)
                {
                    Console.Write("Sendungsnummer: " + delState.ShipmentNumber.Item + "\n" +
                            "Status: " + delState.Status.StatusMessage + "\n" +
                            "Status-Code: " + delState.Status.StatusCode + "\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Jede Taste beendet das Program!");
                String wait = Console.ReadLine();
                Environment.Exit(0);
            }
        }


        private static void runDeleteShipmentDDRequest(SWSServicePortTypeClient port, AuthentificationType auth)
        {
            Console.WriteLine("Sendungsnummer eingeben:");
            String Sendungsnummer = Console.ReadLine();
            DeleteShipmentDDRequest ddRequest = GeschaeftskundenversandRequestBuilder.getDeleteShipmentDDRequest(Sendungsnummer);

            try
            {
                DeleteShipmentResponse delResponse = port.deleteShipmentDD(auth, ddRequest);

                //Response status
                Statusinformation status = delResponse.Status;
                String statusMessage = status.StatusMessage;
                DeletionState[] delStates = delResponse.DeletionState;

                Console.Write("deleteShipmentDDRequest: \n" +
                        "Status-Nachricht: " + statusMessage + "\n");

                foreach (DeletionState delState in delStates)
                {
                    Console.Write("Sendungsnummer: " + delState.ShipmentNumber.Item + "\n" +
                            "Status: " + delState.Status.StatusMessage + "\n" +
                            "Status-Code: " + delState.Status.StatusCode + "\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Jede Taste beendet das Program!");
                String wait = Console.ReadLine();
                Environment.Exit(0);
            }
        }


        private static void runCreateShipmentDDRequest(SWSServicePortTypeClient port, AuthentificationType auth)
        {
            CreateShipmentDDRequest ddRequest = GeschaeftskundenversandRequestBuilder.createDefaultShipmentDDRequest();

            try
            {
                CreateShipmentResponse shResponse = port.createShipmentDD(auth, ddRequest);

                //Response status
                Statusinformation status = shResponse.status;
                String statusCode = status.StatusCode;
                String statusMessage = status.StatusMessage;
                String Shipmentnumber = shResponse.CreationState[0].ShipmentNumber.Item;
                //Label URL
                String labelURL = shResponse.CreationState[0].Labelurl;

                Console.Write("CreateShipmentDDRequest: \n" +
                        "Request Status: Code: " + statusCode + "\n" +
                        "Status-Nachricht: " + statusMessage + "\n" +
                        "Label URL: " + labelURL + "\n" +
                        "Shipmentnumber: " + Shipmentnumber + "\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Jede Taste beendet das Program!");
                String wait = Console.ReadLine();
                Environment.Exit(0);
            }
        }



        private static void runGetLabelDDRequest(SWSServicePortTypeClient port, AuthentificationType auth)
        {

            Console.WriteLine("Sendungsnummer eingeben:");
            String Sendungsnummer = Console.ReadLine();
            
            GetLabelDDRequest ddRequest = GeschaeftskundenversandRequestBuilder.getDefaultLabelDDRequest(Sendungsnummer);

            try
            {
                GetLabelResponse lblResponse = port.getLabelDD(auth, ddRequest);

                //Response status
                Statusinformation status = lblResponse.status;
                String statusMessage = status.StatusMessage;
                LabelData[] lblDataList = lblResponse.LabelData;

                Console.Write("geLabelDDRequest: \n" +
                        "Status-Nachricht: " + statusMessage + "\n");

                foreach (LabelData lblData in lblDataList)
                {
                    Console.Write("Sendungsnummer: " + lblData.ShipmentNumber.Item + "\n" +
                                        "Status: " + lblData.Status.StatusMessage + "\n" +
                                        "Label URL: " + lblData.Labelurl + "\n");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Jede Taste beendet das Program!");
                String wait = Console.ReadLine();
                Environment.Exit(0);
            }
        }

        public static void GetPassword(Dictionary<String, String> credDict, out String username, out String password)
        {
            Console.WriteLine("Bitte EntwicklerID und Password prüfen");
            Console.WriteLine("EntwicklerID: " + credDict[CIG_USERNAME_PROPERTY_NAME]);
            Console.WriteLine("Password:     " + credDict[CIG_PASSWD_PROPERTY_NAME]);
            Console.WriteLine("Sind die Angaben korrekt? (Y/n):");
            String korrekt = "";
            korrekt = Console.ReadLine();
            if (korrekt == "" || korrekt.ToLower() == "y")
            {
                username = credDict[CIG_USERNAME_PROPERTY_NAME];
                password = credDict[CIG_PASSWD_PROPERTY_NAME];
            }
            else
            {
                Console.WriteLine("EntwicklerID eingeben:");
                username = Console.ReadLine();
                Console.WriteLine("Password eingeben:");
                password = Console.ReadLine();
            }
            return;
        }




    }
}
