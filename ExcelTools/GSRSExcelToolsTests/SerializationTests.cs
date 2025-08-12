using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using gov.ncats.ginas.excel.tools.Utils;
using GSRSExcelTools.Model;
using GSRSExcelTools.Model.FDAApplication;
using GSRSExcelTools.Utils;

namespace ginasExcelUnitTests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void GinasToolsConfigurationSerialization()
        {
            GinasToolsConfiguration config = new GinasToolsConfiguration();
            GinasServer serverNcats = new GinasServer();
            serverNcats.ServerName = "tripod.nih.gov prod";
            serverNcats.ServerUrl = "https://tripod.nih.gov/ginas/app/";
            serverNcats.Username = "mitch";
            config.Servers.Add(serverNcats);

            GinasServer serverLocal = new GinasServer();
            serverLocal.ServerName = "localhost 9000 test";
            serverLocal.ServerUrl = "http://localhost:8081/ginas/app/";
            serverLocal.Username = "mitch";
            serverLocal.PrivateKey = "Em6XgmzddkHPL027ltMh";
            config.Servers.Add(serverLocal);

            string configJson = JSTools.GetStringFromGinasToolsConfiguration(config);
            Assert.IsTrue(configJson.Contains(serverLocal.ServerUrl));
            Assert.IsTrue(configJson.Contains(serverNcats.ServerUrl));
            Assert.IsTrue(configJson.Contains(serverNcats.Username));

            System.IO.File.WriteAllText(@"c:\temp\ginas.config.json", configJson);
        }

        [TestMethod]
        public void GetGinasResultFromStringTest()
        {
            string input = "{\"valid\":true,\"message\":\"Success\",\"returned\":{\"uuid\":\"40dd7f46 - 8590 - 4181 - b909 - 958339693459\",\"created\":1527809510843,\"createdBy\":\"admin\",\"lastEdited\":1527809510843,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"definitionType\":\"PRIMARY\",\"definitionLevel\":\"COMPLETE\",\"substanceClass\":\"chemical\",\"status\":\"pending\",\"version\":\"1\",\"names\":[{\"uuid\":\"45c794ce - c6e0 - 285a - 7887 - a8bc331910f4\",\"created\":1527809511360,\"createdBy\":\"admin\",\"lastEdited\":1527809511360,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"name\":\"Thursday\",\"type\":\"CN\",\"domains\":[],\"languages\":[\"ENG\"],\"nameJurisdiction\":[],\"nameOrgs\":[],\"preferred\":true,\"displayName\":true,\"references\":[\"b967a6ce - 3931 - 2b27 - f1b5 - 40d890154dce\"],\"access\":[],\"_self\":\"http://ifxdev.ncats.nih.gov/dev/ginas/app/api/v1/names(45c794ce-c6e0-285a-7887-a8bc331910f4)?view=full\"}],\"codes\":[],\"notes\":[{\"uuid\":\"a10afb4a-bcd7-434b-bc2a-c4d49913a889\",\"created\":1527809511473,\"createdBy\":\"admin\",\"lastEdited\":1527809511473,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"note\":\"[Validation]WARNING:Incorrect number of moieties\",\"references\":[\"227d5676-071b-4d5c-b224-da866400af4f\"],\"access\":[\"admin\"]},{\"uuid\":\"1d25ad42-3f96-4c5a-8c3e-376790787ee7\",\"created\":1527809511505,\"createdBy\":\"admin\",\"lastEdited\":1527809511505,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"note\":\"[Validation]WARNING:Structure has 1 possible duplicate:\n[WK2XYI10QM]IBUPROFEN\",\"references\":[\"227d5676-071b-4d5c-b224-da866400af4f\"],\"access\":[\"admin\"]}],\"properties\":[],\"relationships\":[],\"references\":[{\"uuid\":\"b967a6ce-3931-2b27-f1b5-40d890154dce\",\"created\":1527809511578,\"createdBy\":\"admin\",\"lastEdited\":1527809511578,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"citation\":\"ABCD\",\"docType\":\"WEBSITE\",\"publicDomain\":true,\"tags\":[],\"url\":\"WWW.CNN.COM\",\"access\":[],\"_self\":\"http://ifxdev.ncats.nih.gov/dev/ginas/app/api/v1/references(b967a6ce-3931-2b27-f1b5-40d890154dce)?view=full\"},{\"uuid\":\"227d5676-071b-4d5c-b224-da866400af4f\",\"created\":1527809511619,\"createdBy\":\"admin\",\"lastEdited\":1527809511619,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"citation\":\"GSRS System-generated Validation messages\",\"docType\":\"VALIDATION_MESSAGE\",\"documentDate\":1527809510496,\"publicDomain\":false,\"tags\":[],\"access\":[\"admin\"],\"_self\":\"http://ifxdev.ncats.nih.gov/dev/ginas/app/api/v1/references(227d5676-071b-4d5c-b224-da866400af4f)?view=full\"}],\"tags\":[],\"structure\":{\"id\":\"1b1a7280-8f8d-41b3-a5f0-de833f3b6fef\",\"created\":1527809510584,\"lastEdited\":1527809510593,\"deprecated\":false,\"digest\":\"5b02a5748faee5d648fcdaf68d9ac6490da9e60a\",\"molfile\":\"CC(C)Cc1ccc(cc1)[C@@H](C)C(=O)O\",\"smiles\":\"CC(C)CC1=CC=C(C=C1)[C@@H](C)C(O)=O\",\"formula\":\"C13H18O2\",\"stereoCenters\":1,\"definedStereo\":1,\"ezCenters\":0,\"charge\":0,\"mwt\":206.2808,\"count\":1,\"createdBy\":\"admin\",\"lastEditedBy\":\"admin\",\"hash\":\"JSN8X494Y19Z\",\"self\":\"http://ifxdev.ncats.nih.gov/dev/ginas/app/api/v1/structures(1b1a7280-8f8d-41b3-a5f0-de833f3b6fef)?view=full\",\"references\":[\"b967a6ce-3931-2b27-f1b5-40d890154dce\"],\"access\":[]},\"moieties\":[{\"uuid\":\"333403f1-0c8a-4960-8e4c-e35bc600eec2\",\"created\":1527809511673,\"createdBy\":\"admin\",\"lastEdited\":1527809511673,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"id\":\"333403f1-0c8a-4960-8e4c-e35bc600eec2\",\"digest\":\"b29b410407f6acd2977b5e7996eb76e4751be66a\",\"molfile\":\"\n  Marvin  05311819312D          \n\n 15 15  0  0  1  0            999 V2000\n   -0.3334    1.5125    0.0000 C   0  0  1  0  0  0  0  0  0  1  0  0\n   -1.0479    1.9250    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n    0.3811    1.9250    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n    1.0955    1.5125    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0\n    0.3811    2.7500    0.0000 O   0  0  0  0  0  0  0  0  0  0  0  0\n   -0.3334    0.6875    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n    0.3811    0.2750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n    0.3811   -0.5500    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n   -0.3334   -0.9625    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n   -0.3334   -1.7875    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n    0.3811   -2.2000    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n    1.0955   -1.7875    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n    0.3811   -3.0250    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n   -1.0479   -0.5500    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n   -1.0479    0.2750    0.0000 C   0  0  0  0  0  0  0  0  0  0  0  0\n  1  2  1  1  0  0  0\n  1  3  1  0  0  0  0\n  6  1  1  0  0  0  0\n  3  4  1  0  0  0  0\n  3  5  2  0  0  0  0\n  7  6  1  0  0  0  0\n  6 15  2  0  0  0  0\n  8  7  2  0  0  0  0\n  9  8  1  0  0  0  0\n 10  9  1  0  0  0  0\n  9 14  2  0  0  0  0\n 11 10  1  0  0  0  0\n 12 11  1  0  0  0  0\n 11 13  1  0  0  0  0\n 15 14  1  0  0  0  0\nM  END\n\",\"smiles\":\"CC(C)CC1=CC=C(C=C1)[C@@H](C)C(O)=O\",\"formula\":\"C13H18O2\",\"opticalActivity\":\"UNSPECIFIED\",\"stereoCenters\":1,\"definedStereo\":1,\"ezCenters\":0,\"charge\":0,\"mwt\":206.2808,\"count\":1,\"hash\":\"JSN8X494Y19Z\",\"self\":\"http://ifxdev.ncats.nih.gov/dev/ginas/app/api/v1/structures(333403f1-0c8a-4960-8e4c-e35bc600eec2)?view=full\",\"stereochemistry\":\"ABSOLUTE\",\"references\":[],\"access\":[],\"countAmount\":{\"uuid\":\"8f813d8a-e3ad-442f-afb5-7cc330f14293\",\"created\":1527809511996,\"createdBy\":\"admin\",\"lastEdited\":1527809511996,\"lastEditedBy\":\"admin\",\"deprecated\":false,\"type\":\"MOL RATIO\",\"average\":1,\"units\":\"MOL RATIO\",\"references\":[],\"access\":[]}}],\"_approvalIDDisplay\":\"pending record\",\"_name\":\"Thursday\",\"access\":[\"protected\"],\"_self\":\"http://ifxdev.ncats.nih.gov/dev/ginas/app/api/v1/substances(40dd7f46-8590-4181-b909-958339693459)?view=full\"}}";
            GinasResult result = JSTools.GetGinasResultFromString(input);
            Assert.AreEqual("Success", result.message);
            Assert.IsTrue(result.valid);
        }

        [TestMethod]
        public void TestDeserialization()
        {
            string ginasConfigRaw = "{	\"SelectedServer\" :	    {      \"ServerName\": \"pre prod\",      \"ServerUrl\": \"http://gsrs.preprod.fda.gov/ginas/app/\",      \"Username\": \"MITCHELL.MILLER\",      \"PrivateKey\": \"B72uOzsIDWSD83DE1Y50\",      \"Token\": \"blah\"    },  \"Servers\": [    {      \"ServerName\": \"tripod.nih.gov prod\",      \"ServerUrl\": \"https://tripod.nih.gov/ginas/app/\",      \"Username\": \"mitch\",      \"PrivateKey\": \"blah\",      \"Token\": \"blah\"    },    {      \"ServerName\": \"tripod.nih.gov dev\",      \"ServerUrl\": \"https://tripod.nih.gov/dev/ginas/app/\",      \"Username\": \"admin\",      \"PrivateKey\": \"uG5YRhGiJAz3swRMeri8\",      \"Token\": \"null\"    },    {      \"ServerName\": \"localhost 9000 test\",      \"ServerUrl\": \"http://localhost:9000/ginas/app/\",      \"Username\": \"mitch\",      \"PrivateKey\": \"Em6XgmzddkHPL027ltMh\",      \"Token\": \"null\"    }  ],  \"DebugMode\": false,  \"BatchSize\": 0}";
            GinasToolsConfiguration config= JSTools.GetGinasToolsConfigurationFromString(ginasConfigRaw);
            Assert.AreEqual(3, config.Servers.Count);
            Assert.IsNotNull(config.SelectedServer);
            Console.WriteLine(config.ToString());
        }

        [TestMethod]
        public void TestGetValue()
        {
            ApplicationField field = new ApplicationField();
            field.FieldName = "Submission Date";
            
            field.FieldValue = new DateTime(2018, 2, 14);

            string outputValue = field.GetValue();
            Console.WriteLine(outputValue);
            string expected = "02/14/2018";
            Assert.AreEqual(expected, outputValue);
        }
    }
}
