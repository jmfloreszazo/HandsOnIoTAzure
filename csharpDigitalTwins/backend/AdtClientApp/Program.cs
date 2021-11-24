using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace CreateDTScenario
{
    public class Program
    {
        private static DigitalTwinsClient _client;

        private static async Task Main()
        {
            Uri adtInstanceUrl;
            try
            {
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false)
                    .Build();
                adtInstanceUrl = new Uri(config["instanceUrl"]);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is UriFormatException)
            {
                Console.WriteLine("Have you configured your ADT instance URL in appsettings.json?");
                return;
            }

            var credential = new DefaultAzureCredential();
            _client = new DigitalTwinsClient(adtInstanceUrl, credential);

            var twinList = new List<string>();
            try
            {
                var queryResult = _client.QueryAsync<BasicDigitalTwin>("SELECT * FROM DIGITALTWINS");
                await foreach (var item in queryResult) twinList.Add(item.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*** Error: {ex.Message}");
            }

            foreach (var twinId in twinList)
            {
                await FindAndDeleteOutgoingRelationshipsAsync(twinId);
                await FindAndDeleteIncomingRelationshipsAsync(twinId);
            }

            foreach (var twinId in twinList)
                try
                {
                    await _client.DeleteDigitalTwinAsync(twinId);
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode}");
                }

            string filename;
            string[] filenameArray =
            {
                "Gearbox.json", "Generator.json", "Nacelle.json", "Rotor.json", "Tower.json", "WindFarm.json",
                "WindTurbine.json"
            };
            var consoleAppDir = Path.Combine(Directory.GetCurrentDirectory(), @"Models");
            var dtdList = new List<string>();
            foreach (var t in filenameArray)
            {
                filename = Path.Combine(consoleAppDir, t);
                var r = new StreamReader(filename);
                var dtdl = r.ReadToEnd();
                r.Close();
                dtdList.Add(dtdl);
            }

            try
            {
                await _client.CreateModelsAsync(dtdList);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode}");
            }

            await CreateDigitalTwin("windfarm1", "dtmi:com:idt:wt:windfarm;1");
            await CreateDigitalTwin("windturbine11", "dtmi:com:idt:wt:windturbine;1");
            await CreateDigitalTwin("nacelle11", "dtmi:com:idt:wt:nacelle;1");
            await CreateDigitalTwin("gearbox11", "dtmi:com:idt:wt:gearbox;1");
            await CreateDigitalTwin("generator11", "dtmi:com:idt:wt:generator;1");
            await CreateDigitalTwin("rotor11", "dtmi:com:idt:wt:rotor;1");
            await CreateDigitalTwin("tower11", "dtmi:com:idt:wt:tower;1");
            await CreateDigitalTwin("windturbine12", "dtmi:com:idt:wt:windturbine;1");
            await CreateDigitalTwin("windturbine13", "dtmi:com:idt:wt:windturbine;1");
            await CreateDigitalTwin("windfarm2", "dtmi:com:idt:wt:windfarm;1");
            await CreateDigitalTwin("windturbine21", "dtmi:com:idt:wt:windturbine;1");
            await CreateDigitalTwin("windturbine22", "dtmi:com:idt:wt:windturbine;1");
            await CreateDigitalTwin("windturbine23", "dtmi:com:idt:wt:windturbine;1");
            Thread.Sleep(5000);
            await CreateRelationship(new Guid().ToString(), "windturbine11", "windfarm1", "MemberOf");
            await CreateRelationship(new Guid().ToString(), "nacelle11", "windturbine11", "Houses");
            await CreateRelationship(new Guid().ToString(), "gearbox11", "nacelle11", "PartOf");
            await CreateRelationship(new Guid().ToString(), "generator11", "nacelle11", "PartOf");
            await CreateRelationship(new Guid().ToString(), "rotor11", "windturbine11", "Houses");
            await CreateRelationship(new Guid().ToString(), "tower11", "windturbine11", "Houses");
            await CreateRelationship(new Guid().ToString(), "windturbine12", "windfarm1", "MemberOf");
            await CreateRelationship(new Guid().ToString(), "windturbine13", "windfarm1", "MemberOf");
            await CreateRelationship(new Guid().ToString(), "windturbine21", "windfarm2", "MemberOf");
            await CreateRelationship(new Guid().ToString(), "windturbine22", "windfarm2", "MemberOf");
            await CreateRelationship(new Guid().ToString(), "windturbine23", "windfarm2", "MemberOf");
        }

        public static async Task FindAndDeleteOutgoingRelationshipsAsync(string dtId)
        {
            try
            {
                var basicRelationships = _client.GetRelationshipsAsync<BasicRelationship>(dtId);

                await foreach (var relationship in basicRelationships)
                    await _client.DeleteRelationshipAsync(dtId, relationship.Id);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode}");
            }
        }

        private static async Task FindAndDeleteIncomingRelationshipsAsync(string dtId)
        {
            try
            {
                var incomingRelations = _client.GetIncomingRelationshipsAsync(dtId);

                await foreach (var relationship in incomingRelations)
                    await _client.DeleteRelationshipAsync(relationship.SourceId, relationship.RelationshipId);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"*** Error {ex.Status}/{ex.ErrorCode}");
            }
        }

        public static async Task CreateDigitalTwin(string twinId, string modelId)
        {
            var twinData = new BasicDigitalTwin { Id = twinId, Metadata = { ModelId = modelId } };

            await _client.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData);
        }

        public static async Task CreateRelationship(string relationshipId, string sourceTwinId, string targetTwinId,
            string relationshipName)
        {
            var relationship = new BasicRelationship
            {
                Id = relationshipId, SourceId = sourceTwinId, TargetId = targetTwinId, Name = relationshipName
            };

            await _client.CreateOrReplaceRelationshipAsync(sourceTwinId, relationshipId, relationship);
        }
    }
}