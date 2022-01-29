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

namespace DumpRarities
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Dataminer().Run(args, Go);
        }

        private static void Go(Engine engine)
        {
            var globalsDefinitionClass = engine.GetClass("WillowGame.GlobalsDefinition");
            if (globalsDefinitionClass == null)
            {
                throw new InvalidOperationException();
            }

            dynamic globalsDefinition = engine.Objects.FirstOrDefault(
                o => o.IsA(globalsDefinitionClass) &&
                     o.GetName().StartsWith("Default__") == false);
            if (globalsDefinition == null)
            {
                throw new InvalidOperationException();
            }

            using (var writer = Dataminer.NewDump("Rarities.json"))
            {
                writer.WriteStartObject();

                writer.WritePropertyName(globalsDefinition.GetPath());
                writer.WriteStartObject();

                writer.WritePropertyName("rarity_level_colors");
                writer.WriteStartArray();
                foreach (var rarityLevelColor in globalsDefinition.RarityLevelColors)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("min_level");
                    writer.WriteValue(rarityLevelColor.MinLevel);

                    writer.WritePropertyName("max_level");
                    writer.WriteValue(rarityLevelColor.MaxLevel);

                    writer.WritePropertyName("color");
                    writer.WriteValue((string)rarityLevelColor.Color.GetPath());
                    /*
                    foreach (var color in rarityLevelColor.Color)
                    {
                        writer.WriteStartObject();

                        writer.WritePropertyName("red");
                        writer.WriteValue(color.R);

                        writer.WritePropertyName("green");
                        writer.WriteValue(color.G);

                        writer.WritePropertyName("blue");
                        writer.WriteValue(color.B);

                        writer.WritePropertyName("alpha");
                        writer.WriteValue(color.A);

                        writer.WriteEndObject();
                    }
                    */

                    writer.WritePropertyName("drop_life_span_type");
                    writer.WriteValue(((DropLifeSpanType)rarityLevelColor.DropLifeSpanType).ToString());

                    writer.WritePropertyName("rarity_rating");
                    writer.WriteValue(((ItemRarity)rarityLevelColor.RarityRating).ToString());

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();

                writer.WriteEndArray();
                writer.WriteEndObject();

                writer.Flush();
            }
        }
    }
}
