# XmlDocumentConverter
Converts multiple XML documents into a MDB (Microsoft Access) database whose structure is defined by config.

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

