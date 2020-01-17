﻿using S100Lint.Model.Interfaces;
using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model
{
    public abstract class NodeTypeParserBase : INodeTypeParser
    {
        /// <summary>
        /// Parse nodes and validate them against the catalogue
        /// </summary>
        /// <param name="typeNodes">Nodes to parse</param>
        /// <param name="featureCatalogue">Feature catalogue to use</param>
        /// <returns>List<ReportItem></returns>
        public abstract List<IReportItem> Parse(XmlNodeList typeNodes, XmlDocument featureCatalogue);
    }
}
