<?php

      //Configuration
      $access = " Add License Key Here";
      $userid = " Add User Id Here";
      $passwd = " Add Password Here";
      $accessSchemaFile = " Add AccessRequest Schema File";
      $requestSchemaFile = " Add LocatorRequest Schema File";
      $responseSchemaFile = " Add LocatorResponse Schema File";
      $endpointurl = ' Add URL Here';
      $outputFileName = "XOLTResult.xml";


      try
      {
         //create AccessRequest data object
         $das = SDO_DAS_XML::create("$accessSchemaFile");
    	 $doc = $das->createDocument();
         $root = $doc->getRootDataObject();
         $root->AccessLicenseNumber=$access;
         $root->UserId=$userid;
         $root->Password=$passwd;
         $security = $das->saveString($doc);

         //create LocatorRequest data oject
         $das = SDO_DAS_XML::create("$requestSchemaFile");
         $requestDO = $das->createDataObject('','Request');
         $requestDO->RequestAction='Locator';
         $requestDO->RequestOption='3';
         $trefDO = $das->createDataObject('','TransactionReference');
         $trefDO->CustomerContext = 'Add some text here';
         $requestDO->TransactionReference = $trefDO;

         $doc = $das->createDocument();
         $root = $doc->getRootDataObject();
         $root->Request = $requestDO;

         $oriaddrDO = $das->createDataObject('','OriginAddressType');
         $addrkeyfrmtDO = $das->createDataObject('','AddressKeyFormatType');
      	 $addrkeyfrmtDO->AddressLine = '200 warsaw rd';
         $addrkeyfrmtDO->PoliticalDivision2 = 'Atlanta';
         $addrkeyfrmtDO->PoliticalDivision1 = 'GA' ;
         $addrkeyfrmtDO->PostcodePrimaryLow = '85281';
         $addrkeyfrmtDO->PostcodeExtendedLow = '4510' ;
         $addrkeyfrmtDO->CountryCode = 'US';
         $oriaddrDO->AddressKeyFormat = $addrkeyfrmtDO;
         $oriaddrDO->MaximumListSize = '';
         $root->OriginAddress = $oriaddrDO;

         $translateDO = $das->createDataObject('','TranslateType');
         $translateDO->LanguageCode = 'ENG';
         $root->Translate = $translateDO;

         $unitDO = $das->createDataObject('','UnitOfMeasurementType');
         $unitDO->Code = 'MI';
         $root->UnitOfMeasurement = $unitDO;
         $root->LocationID = '';

         $searchCriteriaDO = $das->createDataObject('','LocationSearchCriteriaType');
         $searchOptionDO1 = $das->createDataObject('','SearchOptionType');
         $searchOptionDO2 = $das->createDataObject('','SearchOptionType');
         $optionTypeDO1 = $das->createDataObject('','CodeType');
         $optionTypeDO2 = $das->createDataObject('','CodeType');
         $optionCodeDO = $das->createDataObject('' , 'OptionCodeType');

         $optionTypeDO1['Code'] = '02';

         $data = array (
               $das->createDataObject('' , 'OptionCodeType'),
	       $das->createDataObject('' , 'OptionCodeType'),
               $das->createDataObject('' , 'OptionCodeType'),
               $das->createDataObject('' , 'OptionCodeType')
         );
         $data[0]['Code'] = '01';
         $data[1]['Code'] = '03';
         $data[2]['Code'] = '04';
         $data[3]['Code'] = '05';

         $searchOptionDO1->OptionType = $optionTypeDO1;
         $searchOptionDO1->OptionCode[] = $data[0];
         $searchOptionDO1->OptionCode[] = $data[1];
         $searchOptionDO1->OptionCode[] = $data[2];
         $searchOptionDO1->OptionCode[] = $data[3];
         $searchCriteriaDO->SearchOption[] = $searchOptionDO1;

         $optionTypeDO2->Code = '03';
         $searchOptionDO2->OptionType = $optionTypeDO2;
         $optionCodeDO->Code = '002';
         $searchOptionDO2->OptionCode = $optionCodeDO;
         $searchCriteriaDO->SearchOption[] = $searchOptionDO2;
         $searchCriteriaDO->MaximumListSize = '';
         $searchCriteriaDO->SearchRadius = '';

         $searchServiceDO = $das->createDataObject('','ServiceSearchType');
         $serviceOptionCodeDO = $das->createDataObject('','CodeType');

         $searchServiceDO->Time = '1600';
         $data2 = array
         (
             $das->createDataObject('','CodeType'),
             $das->createDataObject('','CodeType')
         );
         $data2[0]['Code'] = '01';
         $data2[1]['Code'] = '02';
         $searchServiceDO->ServiceCode[] = $data2[0];
         $searchServiceDO->ServiceCode[] = $data2[1];

         $serviceOptionCodeDO->Code = '01';
         $searchServiceDO->ServiceOptionCode = $serviceOptionCodeDO;
         $searchCriteriaDO->ServiceSearch = $searchServiceDO;
         $root->LocationSearchCriteria = $searchCriteriaDO;
         $request = $das->saveString($doc);

         //create Post request
         $form = array
         (
             'http' => array
             (
                 'method' => 'POST',
                 'header' => 'Content-type: application/x-www-form-urlencoded',
                 'content' => "$security$request"
             )
         );

         //print form request
         print_r($form);


         $request = stream_context_create($form);
         $browser = fopen($endpointurl , 'rb' , false , $request);
         if(!$browser)
         {
             throw new Exception("Connection failed.");
         }

         //get response
         $response = stream_get_contents($browser);
         fclose($browser);

         if($response == false)
         {
            throw new Exception("Bad data.");
         }
         else
         {
            //save request and response to file
  	    $fw = fopen($outputFileName,'w');
            fwrite($fw , "Response: \n" . $response . "\n");
            fclose($fw);

            //get response status
            $resp = new SimpleXMLElement($response);
            echo $resp->Response->ResponseStatusDescription . "\n";
         }

      }
      catch(SDOException $sdo)
      {
      	 echo $sdo;
      }
      catch(Exception $ex)
      {
      	echo $ex;
      }

?>

