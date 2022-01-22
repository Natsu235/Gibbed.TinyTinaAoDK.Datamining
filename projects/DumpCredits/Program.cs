﻿/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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
using System.Linq;
using Gibbed.Unreflect.Core;
using Dataminer = TinyTinaAoDKDatamining.Dataminer;

namespace DumpCredits
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Dataminer().Run(args, Go);
        }

        private static void Go(Engine engine)
        {
            var creditsGfxDefinitionClass = engine.GetClass("WillowGame.CreditsGFxDefinition");
            if (creditsGfxDefinitionClass == null)
            {
                throw new InvalidOperationException();
            }

            dynamic creditsGfxDefinition = engine.Objects.FirstOrDefault(
                o => o.IsA(creditsGfxDefinitionClass) &&
                     o.GetName().StartsWith("Default__") == false);
            if (creditsGfxDefinition == null)
            {
                Console.WriteLine("Credits object is missing.");
                return;
            }

            using (var writer = Dataminer.NewDump("Credits.json"))
            {
                writer.WriteStartArray();
                foreach (var line in creditsGfxDefinition.CreditData)
                {
                    writer.WriteValue(line.Text);
                }
                writer.WriteEndArray();
                writer.Flush();
            }
        }
    }
}
