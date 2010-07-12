﻿//
// $Id$
//
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is the IDPicker project.
//
// The Initial Developer of the Original Code is Matt Chambers.
//
// Copyright 2010 Vanderbilt University
//
// Contributor(s): Surendra Dasari
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml;
using System.Text;
using System.Data.SQLite;
using System.Linq;

using NHibernate;
using NHibernate.Linq;

namespace IDPicker.DataModel
{
    public class Exporter : IDisposable
    {
        ISession session;

        public Exporter (string idpDbFilepath)
        {
            var sessionFactory = SessionFactoryFactory.CreateSessionFactory(idpDbFilepath, false, true);
            var session = sessionFactory.OpenSession();
        }

        public Exporter (ISession session)
        {
            this.session = session;
        }

        public void WriteIdpXml (bool includeProteinSequences)
        {
            // get the set of distinct analysis/source pairs
            var analysisSourcePairs = session.CreateQuery("SELECT psm.Analysis, psm.Spectrum.Source " +
                                                          "FROM PeptideSpectrumMatch psm " +
                                                          "GROUP BY psm.Analysis.id, psm.Spectrum.Source.id")
                                             .List<object[]>();

            // get the set of sources that have been in more than one analysis
            var multiAnalyzedSources = session.CreateQuery("SELECT psm.Spectrum.Source.id " +
                                                           "FROM PeptideSpectrumMatch psm " +
                                                           "GROUP BY psm.Spectrum.Source.id " +
                                                           "HAVING COUNT(DISTINCT psm.Analysis.id) > 1")
                                              .List<long>();

            foreach (object[] analysisSourcePair in analysisSourcePairs)
            {
                Analysis a = analysisSourcePair[0] as Analysis;
                SpectrumSource ss = analysisSourcePair[1] as SpectrumSource;

                var settings = new XmlWriterSettings()
                {
                    NewLineHandling = NewLineHandling.Replace,
                    NewLineChars = "\n",
                    Indent = true,
                    IndentChars = "\t"
                };

                string outputSuffix = ".idpXML";
                if (multiAnalyzedSources.Contains(ss.Id.GetValueOrDefault()))
                    outputSuffix = "-" + a.Software.Name + "-" + a.StartTime.ToUniversalTime().ToString("yyyyMMddTHHmmss'Z'") + outputSuffix;

                var writer = XmlWriter.Create(ss.Name + outputSuffix, settings);
                writer.WriteStartDocument();
                writer.WriteStartElement("idPickerPeptides");

                #region <proteinIndex>
                var proteinRows = session.CreateQuery("SELECT DISTINCT pro " +
                                                      "FROM Protein pro " +
                                                      "JOIN pro.Peptides pi " +
                                                      "JOIN pi.Peptide.Matches psm " +
                                                      "WHERE psm.Spectrum.Source.id = " + ss.Id)
                                         .List<Protein>();
                writer.WriteStartElement("proteinIndex");
                writer.WriteAttribute("count", proteinRows.Count);
                foreach (Protein pro in proteinRows)
                {
                    writer.WriteStartElement("protein");
                    writer.WriteAttribute("id", pro.Id);
                    writer.WriteAttribute("locus", pro.Accession);
                    writer.WriteAttribute("decoy", 0);
                    writer.WriteAttribute("length", pro.Length);
                    writer.WriteAttribute("description", pro.Description);
                    if (includeProteinSequences) writer.WriteAttribute("sequence", pro.Sequence);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); // proteinIndex
                #endregion

                #region <peptideIndex>
                var peptideRows = session.CreateQuery("SELECT DISTINCT psm.Peptide " +
                                                      "FROM PeptideSpectrumMatch psm " +
                                                      "WHERE psm.Spectrum.Source.id = " + ss.Id)
                                         .List<Peptide>();
                writer.WriteStartElement("peptideIndex");
                writer.WriteAttribute("count", peptideRows.Count);
                foreach (Peptide pep in peptideRows)
                {
                    writer.WriteStartElement("peptide");
                    writer.WriteAttribute("id", pep.Id);
                    writer.WriteAttribute("sequence", pep.Sequence);
                    writer.WriteAttribute("mass", pep.MonoisotopicMass);
                    writer.WriteAttribute("unique", pep.Instances.Count > 1 ? 0 : 1);
                    writer.WriteAttribute("NTerminusIsSpecific", pep.Instances[0].NTerminusIsSpecific ? 1 : 0);
                    writer.WriteAttribute("CTerminusIsSpecific", pep.Instances[0].CTerminusIsSpecific ? 1 : 0);

                    foreach (PeptideInstance pi in pep.Instances)
                    {
                        writer.WriteStartElement("locus");
                        writer.WriteAttribute("id", pi.Protein.Id);
                        writer.WriteAttribute("offset", pi.Offset);
                        writer.WriteEndElement(); // locus
                    }

                    writer.WriteEndElement(); // peptide
                }
                writer.WriteEndElement(); // peptideIndex
                #endregion

                writer.WriteStartElement("spectraSources");
                writer.WriteAttribute("count", 1);
                {
                    writer.WriteStartElement("spectraSource");
                    writer.WriteAttribute("name", ss.Name);
                    writer.WriteAttribute("count", ss.Spectra.Count);
                    {
                        #region <processingEventList>
                        writer.WriteStartElement("processingEventList");
                        writer.WriteAttribute("count", 2);
                        {
                            writer.WriteStartElement("processingEvent");
                            writer.WriteAttribute("type", "identification");
                            writer.WriteAttribute("start", a.StartTime.ToString("MM/dd/yyyy@HH:mm:ss"));
                            writer.WriteAttribute("end", a.StartTime.ToString("MM/dd/yyyy@HH:mm:ss")); // TODO: do we need end time?
                            writer.WriteAttribute("params", a.Parameters.Count);

                            foreach (AnalysisParameter ap in a.Parameters)
                            {
                                writer.WriteStartElement("processingParam");
                                writer.WriteAttribute("name", ap.Name);
                                writer.WriteAttribute("value", ap.Value);
                                writer.WriteEndElement(); // processingParam
                            }
                            writer.WriteEndElement(); // processingEvent

                            writer.WriteStartElement("processingEvent");
                            writer.WriteAttribute("type", "validation");
                            writer.WriteAttribute("start", DateTime.Now.ToString("MM/dd/yyyy@HH:mm:ss"));
                            writer.WriteAttribute("end", DateTime.Now.ToString("MM/dd/yyyy@HH:mm:ss"));
                            writer.WriteAttribute("params", a.Parameters.Count);

                            var validationParameters = new KeyValuePair<string, string>[]
                            {
                                new KeyValuePair<string, string>("software name", "IDPicker"),
                                new KeyValuePair<string, string>("software version", IDPicker.Util.Version ),
                                new KeyValuePair<string, string>("MaxFDR", "1"),
                                new KeyValuePair<string, string>("MaxResultRank", "1"),
                            };

                            foreach (var ap in validationParameters)
                            {
                                writer.WriteStartElement("processingParam");
                                writer.WriteAttribute("name", ap.Key);
                                writer.WriteAttribute("value", ap.Value);
                                writer.WriteEndElement(); // processingParam
                            }
                            writer.WriteEndElement(); // processingEvent
                        }
                        writer.WriteEndElement(); // processingEventList
                        #endregion

                        foreach (Spectrum s in ss.Spectra)
                        {
                            var psmList = s.Matches.Where(o => o.Analysis.Id == a.Id && o.Rank == 1).ToList();
                            var distinctChargeStates = psmList.Select(o => o.Charge).Distinct();

                            foreach (int charge in distinctChargeStates)
                            {
                                var psmListAtCharge = psmList.Where(o => o.Charge == charge).ToList();

                                #region <spectrum ...>
                                writer.WriteStartElement("spectrum");
                                writer.WriteAttribute("id", s.NativeID);
                                writer.WriteAttribute("index", s.Index);
                                writer.WriteAttribute("z", charge);
                                writer.WriteAttribute("mass", (s.PrecursorMZ * charge) - (pwiz.CLI.chemistry.Proton.Mass * charge));
                                // TODO: writer.WriteAttribute("time", s.ScanStartTime);
                                writer.WriteAttribute("results", 1); // rank 1 only
                                {
                                    writer.WriteStartElement("result");
                                    writer.WriteAttribute("rank", 1);
                                    writer.WriteAttribute("FDR", psmListAtCharge[0].QValue);
                                    writer.WriteAttribute("ids", psmListAtCharge.Count);

                                    foreach (PeptideSpectrumMatch psm in psmListAtCharge)
                                    {
                                        writer.WriteStartElement("id");
                                        writer.WriteAttribute("peptide", psm.Peptide.Id);

                                        if (psm.Modifications.Count > 0)
                                        {
                                            var mods = new List<string>();
                                            foreach (PeptideModification pm in psm.Modifications)
                                            {
                                                string offset;
                                                switch (pm.Offset)
                                                {
                                                    case int.MinValue: offset = "n"; break;
                                                    case int.MaxValue: offset = "c"; break;
                                                    default: offset = pm.Offset.ToString(); break;
                                                }
                                                mods.Add(String.Format("{0}:{1}", offset, pm.Modification.MonoMassDelta));
                                            }
                                            writer.WriteAttribute("mods", String.Join(" ", mods.ToArray()));
                                        }

                                        writer.WriteEndElement(); // id
                                    }
                                    writer.WriteEndElement(); // result
                                }
                                writer.WriteEndElement(); // spectrum
                                #endregion
                            }
                        }
                    }
                    writer.WriteEndElement(); // spectraSource
                }

                writer.WriteEndElement(); // spectraSources

                writer.WriteEndDocument();
                writer.Close();
            }
        }

        #region IDisposable Members

        public void Dispose ()
        {
        }

        #endregion
    }

    public static class ExportExtensions
    {
        public static void WriteAttribute<T> (this XmlWriter writer, string name, T value)
        {
            writer.WriteAttributeString(name, value.ToString());
        }
    }
}