/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Gibbed.Unreflect.Core;
using Dataminer = TinyTinaAoDKDatamining.Dataminer;

namespace DumpTravelStations
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Dataminer().Run(args, Go);
        }

        private static void Go(Engine engine)
        {
            var travelStationDefinitionClass = engine.GetClass("WillowGame.TravelStationDefinition");
            var fastTravelStationDefinitionClass = engine.GetClass("WillowGame.FastTravelStationDefinition");
            var levelTravelStationDefinitionClass = engine.GetClass("WillowGame.LevelTravelStationDefinition");
            var fastTravelStationsListOrderClass = engine.GetClass("WillowGame.FastTravelStationsListOrder");

            if (travelStationDefinitionClass == null ||
                fastTravelStationDefinitionClass == null ||
                levelTravelStationDefinitionClass == null ||
                fastTravelStationsListOrderClass == null)
            {
                throw new InvalidOperationException();
            }

            using (var writer = Dataminer.NewDump("Travel Stations.json"))
            {
                writer.WriteStartObject();

                var travelStationDefinitions = engine.Objects
                    .Where(o =>
                           (o.IsA(travelStationDefinitionClass) == true ||
                            o.IsA(fastTravelStationDefinitionClass) == true ||
                            o.IsA(levelTravelStationDefinitionClass) == true) &&
                           o.GetName().StartsWith("Default__") == false)
                    .OrderBy(o => o.GetPath());
                foreach (dynamic travelStationDefinition in travelStationDefinitions)
                {
                    UnrealClass uclass = travelStationDefinition.GetClass();
                    if (uclass.Path != "WillowGame.FastTravelStationDefinition" &&
                        uclass.Path != "WillowGame.LevelTravelStationDefinition")
                    {
                        throw new InvalidOperationException();
                    }

                    writer.WritePropertyName(travelStationDefinition.GetPath());
                    writer.WriteStartObject();

                    if (uclass.Path != "WillowGame.TravelStationDefinition")
                    {
                        writer.WritePropertyName("$type");
                        writer.WriteValue(uclass.Name);
                    }

                    writer.WritePropertyName("resource_name");
                    writer.WriteValue(travelStationDefinition.GetName());

                    string stationLevelName = travelStationDefinition.StationLevelName;
                    if (string.IsNullOrEmpty(stationLevelName) == false)
                    {
                        writer.WritePropertyName("level_name");
                        writer.WriteValue(stationLevelName);
                    }

                    var dlcExpansion = travelStationDefinition.DlcExpansion;
                    if (dlcExpansion != null)
                    {
                        writer.WritePropertyName("dlc_expansion");
                        writer.WriteValue(dlcExpansion.GetPath());
                    }

                    if (travelStationDefinition.PreviousStation != null)
                    {
                        writer.WritePropertyName("previous_station");
                        writer.WriteValue(travelStationDefinition.PreviousStation.GetPath());
                    }

                    string stationDisplayName = travelStationDefinition.StationDisplayName;
                    if (string.IsNullOrEmpty(stationDisplayName) == false)
                    {
                        writer.WritePropertyName("station_display_name");
                        writer.WriteValue(stationDisplayName);
                    }

                    var missionDependencies = ((IEnumerable<dynamic>)travelStationDefinition.MissionDependencies)
                        .Where(md => md.MissionDefinition != null)
                        .OrderBy(md => md.MissionDefinition.GetPath())
                        .ToArray();
                    if (missionDependencies.Length > 0)
                    {
                        writer.WritePropertyName("mission_dependencies");
                        writer.WriteStartArray();

                        foreach (var missionDependency in missionDependencies)
                        {
                            writer.WriteStartObject();

                            writer.WritePropertyName("mission_definition");
                            writer.WriteValue(missionDependency.MissionDefinition.GetPath());

                            writer.WritePropertyName("mission_status");
                            writer.WriteValue(((MissionStatus)missionDependency.MissionStatus).ToString());

                            if ((bool)missionDependency.bIsObjectiveSpecific == true)
                            {
                                writer.WritePropertyName("is_objective_specific");
                                writer.WriteValue(true);

                                if (missionDependency.MissionObjective != null)
                                {
                                    writer.WritePropertyName("objective_definition");
                                    writer.WriteValue(missionDependency.MissionObjective.GetPath());
                                }

                                writer.WritePropertyName("objective_status");
                                writer.WriteValue(
                                    ((ObjectiveDependencyStatus)missionDependency.ObjectiveStatus).ToString());
                            }

                            writer.WriteEndObject();
                        }

                        writer.WriteEndArray();
                    }

                    if (uclass == fastTravelStationDefinitionClass)
                    {
                        writer.WritePropertyName("initially_active");
                        writer.WriteValue((bool)travelStationDefinition.bInitiallyActive);

                        writer.WritePropertyName("send_only");
                        writer.WriteValue((bool)travelStationDefinition.bSendOnly);

                        string stationDescription = travelStationDefinition.StationDescription;
                        if (string.IsNullOrEmpty(stationDescription) == false &&
                            stationDescription != "No Description" &&
                            stationDescription != stationDisplayName)
                        {
                            writer.WritePropertyName("description");
                            writer.WriteValue(stationDescription);
                        }

                        string stationSign = travelStationDefinition.StationSign;
                        if (string.IsNullOrEmpty(stationSign) == false &&
                            stationSign != stationDisplayName)
                        {
                            writer.WritePropertyName("sign");
                            writer.WriteValue(stationSign);
                        }

                        if (travelStationDefinition.InaccessibleObjective != null)
                        {
                            writer.WritePropertyName("inaccessible_objective");
                            writer.WriteValue(travelStationDefinition.InaccessibleObjective.GetPath());
                        }

                        if (travelStationDefinition.AccessibleObjective != null)
                        {
                            writer.WritePropertyName("accessible_objective");
                            writer.WriteValue(travelStationDefinition.AccessibleObjective.GetPath());
                        }
                    }
                    else if (uclass == levelTravelStationDefinitionClass)
                    {
                        if (travelStationDefinition.DestinationStationDefinition != null)
                        {
                            writer.WritePropertyName("destination_station");
                            writer.WriteValue(travelStationDefinition.DestinationStationDefinition.GetPath());
                        }

                        string displayName = travelStationDefinition.DisplayName;
                        if (string.IsNullOrEmpty(displayName) == false &&
                            displayName != "No Description" &&
                            displayName != stationDisplayName)
                        {
                            writer.WritePropertyName("display_name");
                            writer.WriteValue(displayName);
                        }
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.Flush();
            }

            var fastTravelStationsListOrders = engine.Objects
                .Where(o =>
                       (o.IsA(fastTravelStationsListOrderClass) == true) &&
                       o.GetName().StartsWith("Default__") == false)
                .OrderBy(o => o.GetPath());
            using (var writer = Dataminer.NewDump("Fast Travel Station Ordering.json"))
            {
                writer.WriteStartObject();

                foreach (dynamic fastTravelStationsListOrder in fastTravelStationsListOrders)
                {
                    writer.WritePropertyName(fastTravelStationsListOrder.GetPath());
                    writer.WriteStartObject();

                    writer.WritePropertyName("stations");
                    writer.WriteStartArray();
                    foreach (var fastTravelStationDefinition in fastTravelStationsListOrder.FastTravelStationOrderList)
                    {
                        writer.WriteValue(fastTravelStationDefinition.GetPath());
                    }
                    writer.WriteEndArray();

                    var dlcExpansion = fastTravelStationsListOrder.DlcExpansion;
                    if (dlcExpansion != null)
                    {
                        writer.WritePropertyName("dlc_expansion");
                        writer.WriteValue(dlcExpansion.GetPath());
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }
        }
    }
}
