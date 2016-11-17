﻿using Neo4j.Driver.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DT.DTGraph {
    public class DTNeoHelper : IDisposable {
        public void Dispose() {
            
        }

        public string GetFrameTreeForConstructor(string frameName) {
            string jsonFrameTree = null;
            using (var session = MvcApplication.NeoDTDriver.Session()) {
                string cypher = @"MATCH (f:Frame{name: {frameName} })-[hs:HAS_SLOT]->(s:Slot)
OPTIONAL MATCH (s)-[ht:HAS_TERM]->(t:Term)
WITH f,s,hs,ht, t
ORDER BY ht.index
WITH f,s,hs,
CASE WHEN t IS NOT NULL THEN collect({nodetype: 'term', origTitle: t.name, title: t.name, index: ht.index, parent: s.name}) ELSE [] END as c_terms
ORDER BY hs.index
WITH collect({nodetype: 'slot', origTitle: s.name, title: s.name, index:hs.index,  nodes: c_terms, parent: f.name}) as c_slotrows
MATCH (f:Frame{name: 'Complaints' })-[ltf:LINK_TO_FRAME]->(sf:Frame)
WITH ltf, f, c_slotrows + collect({nodetype: 'framelink', origTitle: sf.name, title: sf.name, index: ltf.index, parent: 'frame'}) as c_allrows
WITH f, c_allrows
UNWIND c_allrows AS u_allrows
WITH f, u_allrows
ORDER BY u_allrows.index
WITH {title:f.name, nodes:collect(u_allrows)} AS frame
RETURN frame.nodes
";
                IStatementResult result = session.Run(cypher, new Dictionary<string, object> { { "frameName", frameName } });
                IRecord frameRecord = result.FirstOrDefault();
                if (frameRecord != null) {
                    jsonFrameTree = JsonConvert.SerializeObject(frameRecord.Values["frame.nodes"]);
                }
            }
            return jsonFrameTree;
        }
    }
}