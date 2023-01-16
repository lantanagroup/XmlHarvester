<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2" xmlns:trif="http://www.lantanagroup.com">
    <xsl:output indent="yes" />
    
    <xsl:template match="/">
        <config tableName="Document">
            <namespace prefix="cda" uri="urn:hl7-org:v3" />
            <namespace prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance" />
            <namespace prefix="sdtc" uri="urn:hl7-org:sdtc" />
            
            <column name="TemplateId" heading="Related Template ID">/cda:ClinicalDocument/cda:templateId/@root</column>
            <column name="DocumentId">/cda:ClinicalDocument/cda:id/concat(@extension, ' (', @root, ')')</column>
            
            <xsl:apply-templates select="//trif:Template[@templateType='section']" />
        </config>
    </xsl:template>
    
    <xsl:template name="last-index-of">
        <xsl:param name="txt"/>
        <xsl:param name="remainder" select="$txt"/>
        <xsl:param name="delimiter" select="' '"/>
        
        <xsl:choose>
            <xsl:when test="contains($remainder, $delimiter)">
                <xsl:call-template name="last-index-of">
                    <xsl:with-param name="txt" select="$txt"/>
                    <xsl:with-param name="remainder" select="substring-after($remainder, $delimiter)"/>
                    <xsl:with-param name="delimiter" select="$delimiter"/>
                </xsl:call-template>      
            </xsl:when>
            <xsl:otherwise>
                <xsl:variable name="lastIndex" select="string-length(substring($txt, 1, string-length($txt)-string-length($remainder)))+1"/>
                <xsl:choose>
                    <xsl:when test="string-length($remainder)=0">
                        <xsl:value-of select="string-length($txt)"/>
                    </xsl:when>
                    <xsl:when test="$lastIndex>0">
                        <xsl:value-of select="($lastIndex - string-length($delimiter))"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select="0"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
    <xsl:template match="trif:Template[@templateType='section']">
        <xsl:variable name="hl7iilast">
            <xsl:call-template name="last-index-of">
                <xsl:with-param name="txt" select="@identifier" />
                <xsl:with-param name="delimiter" select="':'"></xsl:with-param>
            </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="hl7ii" select="substring(@identifier, 11, $hl7iilast - 11)" />
        <xsl:variable name="oid">
            <xsl:choose>
                <xsl:when test="starts-with(@identifier, 'urn:oid:')">
                    <xsl:value-of select="substring(@identifier, 9)" />
                </xsl:when>
                <xsl:when test="starts-with(@identifier, 'urn:hl7ii:')">
                    <xsl:value-of select="$hl7ii" />
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="@identifier" />
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        
        <group>
            <xsl:attribute name="tableName" select="@bookmark" />
            <xsl:attribute name="context">
                <xsl:value-of select="concat('//cda:section[cda:templateId/@root=''', $oid, ''']')" />
            </xsl:attribute>
            
            <xsl:apply-templates select=".//trif:Constraint[not(descendant::trif:SingleValueCode | descendant::trif:ValueSet | descendant::trif:CodeSystem) and @context != 'component']" />
        </group>
    </xsl:template>
    
    <xsl:template match="trif:Constraint">
        <xsl:variable name="name">
            <xsl:choose>
                <xsl:when test="starts-with(@context, '@')">
                    <xsl:value-of select="substring(@context, 2)" />
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="@context" />
                </xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        
        <column>
            <xsl:attribute name="name" select="$name" />
        </column>
    </xsl:template>
</xsl:stylesheet>