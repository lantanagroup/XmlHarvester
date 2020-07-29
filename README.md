# XmlDocumentConverter
This is an open source tool freely available for anyone to use. The C# source code is also available for anyone to download and configure. This tool extracts discrete data from standard CDA XML files, such as eICR and QRDA files, and stores these data elements in a database. Additionally, the parser is configurable to enable/disable schema (.xsd) and schematron (.sch) validation against provided validation files.

## Features

* Convert XML to MDB
* Convert XML to XLSX
* Specify structure of MDB and XLSX via config
* Use XPATH to define where data comes from in source XML documents
* User interface to specify source and destination directories, and review logs

## Config

The structure of the MS Access DB and where the data for each table/column comes from in the XML files is defined in a `MappingConfig.xml` file.

Example:

```xml
<config tableName="document">
  <namespace prefix="cda" uri="urn:hl7-org:v3" />
  
  <column name="colName">XPATH</column>
  
  <group tableName="author" columnPrefix="author" contex="XPATH">
    <column name="colName">XPATH</column>
    
    <!-- nested groups -->
  </group>
</config>
```

| Config Property | Description |
| --------------- | ----------- |
| config | This is the root element for the entire XML configuration. This represents each XML document processed by the tool. |
| config.tableName | The table in the MDB that should hold document-level information. |
| config.namespace | Namespace definitions that are used to process XPATH. If XPATH includes namespaces, the namespaces should be defined here. |
| config.column, group.column | A column in a table. For config.column, XPATH is in the context of the entire document. For group.column the context is within the context specified for the group. |
| config.group | A table that is related to the document table. Data within this table is based on the data specified within the `context` attribute. |
| config.group.group | Nested tables, related to the parent table. |

## Command Line Interface

A CLI is available to run the tool from the command line (or automatically from another process).

The general format of the CLI is as follows:

`XmlDocConverterCli.exe <command> [options]`

Help can be provided by the CLI tool itself:

`XmlDocConverterCli.exe [command] --help`

### Command: xlsx

| Parameter | Description |
| --------- | ----------- |
| -c, --config | Required. The location of the mapping config XML file. |
| -i, --input | Required. The directory that contains the input XML files. |
| -o, --output | Required. The directory where output (XLSX) files should go. |
| -m, --move | The directory to move input files to once they are done being processed. |
| -x, --xsd | The path to an XML Schema (XSD) that should be used to validate the structure of each XMl document processed. |
| -s, --sch | The path to an ISO Schematron (SCH) file that should be used to validate the content of each XMl document processed. |

### Command: mdb

| Parameter | Description |
| --------- | ----------- |
| -c, --config | Required. The location of the mapping config XML file. |
| -i, --input | Required. The directory that contains the input XML files. |
| -o, --output | Required. The directory where output (MDB) files should go. |
| -m, --move | The directory to move input files to once they are done being processed. |
| -x, --xsd | The path to an XML Schema (XSD) that should be used to validate the structure of each XMl document processed. |
| -s, --sch | The path to an ISO Schematron (SCH) file that should be used to validate the content of each XMl document processed. |

### Command: db2

| Parameter | Description |
| --------- | ----------- |
| -c, --config | Required. The location of the mapping config XML file. |
| -i, --input | Required. The directory that contains the input XML files. |
| -u, --username | Required. The authenticated username to access the DB. |
| -p, --password | Required. The authenticated password to access the DB. |
| -d, --database | (Default: xdc) The name of the database to convert/output to. |
| -m, --move | The directory to move input files to once they are done being processed. |
| -x, --xsd | The path to an XML Schema (XSD) that should be used to validate the structure of each XMl document processed. |
| -s, --sch | The path to an ISO Schematron (SCH) file that should be used to validate the content of each XMl document processed. |

## DB2 Conversion

### Pre-requisites

To convert to a DB2 database, the machine that executes the XmlDocumentConverter must have the [IBM Data Server Runtime Client](https://www.ibm.com/support/pages/download-initial-version-115-clients-and-drivers) installed as a dependency. In addition to installing the runtime client, it must be configured to describe the database you want to export to. In the below example, "xdc" is the database that is being used for conversion.

**Example db2dsdriver.cfg**

```xml
<configuration>
   <!-- Multi-line comments are not supported -->
   <dsncollection>
      <dsn alias="dock" name="docker" description="alias1_description" host="localhost" port="50000"/>
   </dsncollection>
   <databases>
      <database name="xdc" host="localhost" port="50000">
         <parameter name="CurrentSchema" value="xdc"/>
         <wlb>
            <parameter name="enableWLB" value="true"/>
            <parameter name="maxTransports" value="50"/>
         </wlb>
         <acr>
            <parameter name="enableACR" value="true"/>
         </acr>
         <specialregisters>
            <parameter name="CURRENT DEGREE" value="'ANY'"/>
         </specialregisters>
      </database>
   </databases>
</configuration>
```

### Running

To output/convert to a DB2 database, specify the named database in the "Database to connect to" field, as well as the username and password of the user. The name of the database should match the  name in the `db2dsdriver.cfg` file (ex: "xdc" in the above example).

When conversion begins, it will first check to ensure that the DB2 database has the schema as described by the mapping configuration. If a table already exists and the columns do *not* align with the mapping config, an error will be produced. If the table does not already exist, it will be automatically created by the XmlDocumentConverter tool.
