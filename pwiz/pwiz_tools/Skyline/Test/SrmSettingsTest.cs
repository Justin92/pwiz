/*
 * Original author: Brendan MacLean <brendanx .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.SkylineTest
{        
    /// <summary>
    /// This is a test class for SrmSettingsTest and is intended
    /// to contain all SrmSettingsTest Unit Tests
    /// </summary>
    [TestClass]
    public class SrmSettingsTest
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        private const string XML_DIRECTIVE = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n";

        /// <summary>
        /// Simple test of serializing the default SrmSettings, reloading
        /// and ensuring consistency.
        /// </summary>
        [TestMethod]
        public void SettingsSerializeDefaultsTest()
        {
            AssertEx.Serializable(SrmSettingsList.GetDefault(), AssertEx.SettingsCloned);
        }

        /// <summary>
        /// Test of deserializing current settings.
        /// </summary>
        [TestMethod]
        public void SettingsSerializeCurrentTest()
        {
            AssertEx.Serializable(AssertEx.Deserialize<SrmSettings>(SETTINGS_CURRENT), 3, AssertEx.SettingsCloned);
        }

        private const string SETTINGS_CURRENT =
            "<settings_summary name=\"Default\">\n" +
            "    <peptide_settings>\n" +
            "        <enzyme name=\"LysN promisc\" cut=\"KASR\" no_cut=\"\" sense=\"N\" />\n" +
            "        <digest_settings max_missed_cleavages=\"1\" exclude_ragged_ends=\"true\" />\n" +
            "        <peptide_prediction>\n" +
            "            <predict_retention_time name=\"Bovine Standard (100A)\" calculator=\"SSRCalc 3.0 (100A)\"\n" +
            "                time_window=\"13.6\">\n" +
            "                <regression_rt slope=\"1.681\" intercept=\"-6.247\" />\n" +
            "            </predict_retention_time>\n" +
            "        </peptide_prediction>\n" +
            "        <peptide_filter start=\"0\" min_length=\"5\" max_length=\"30\" min_transtions=\"4\"\n" +
            "            auto_select=\"True\">\n" +
            "            <peptide_exclusions>\n" +
            "                <exclusion name=\"Met\" regex=\"[M]\" />\n" +
            "                <exclusion name=\"NXT/NXS\" regex=\"N.[TS]\" />\n" +
            "                <exclusion name=\"D Runs\" regex=\"DDDD\" />\n" +
            "            </peptide_exclusions>\n" +
            "        </peptide_filter>\n" +
            "        <peptide_libraries />\n" +
            "        <peptide_modifications>\n" +
            "            <static_modifications>\n" +
            "                <static_modification name=\"Test2\" aminoacid=\"M\" massdiff_monoisotopic=\"5\"\n" +
            "                    massdiff_average=\"5.1\" />\n" +
            "                <static_modification name=\"Test3\" aminoacid=\"K\" terminus=\"N\"\n" +
            "                    formula=\"CH3ON2\" />\n" +
            "            </static_modifications>\n" +
            "        </peptide_modifications>\n" +
            "    </peptide_settings>\n" +
            "    <transition_settings>\n" +
            "        <transition_prediction precursor_mass_type=\"Average\" fragment_mass_type=\"Average\">\n" +
            "            <predict_collision_energy name=\"ABI\">\n" +
            "                <regression_ce charge=\"2\" slope=\"0.0431\" intercept=\"4.7556\" />\n" +
            "            </predict_collision_energy>\n" +
            "            <predict_declustering_potential name=\"Test1\" slope=\"0.5\" intercept=\"5\" />\n" +
            "        </transition_prediction>\n" +
            "        <transition_filter precursor_charges=\"2,3\" product_charges=\"1,2\"\n" +
            "            fragment_range_first=\"y3\" fragment_range_last=\"last y-ion - 1\"\n" +
            "            include_n_proline=\"true\" include_c_glu_asp=\"true\" auto_select=\"true\" />\n" +
            "        <transition_libraries ion_match_tolerance=\"0.5\" ion_count=\"3\" pick_from=\"all\" />\n" +
            "        <transition_integration/>" +
            "        <transition_instrument min_mz=\"52\" max_mz=\"1503\" />\n" +
            "    </transition_settings>\n" +
            "</settings_summary>";

        /// <summary>
        /// Test of deserializing v0.1 settings, by deserializing versions written
        /// by v0.1 and the current code, and checking for equality.
        /// </summary>
        [TestMethod]
        public void SettingsSerialize_0_1_Test()
        {
// ReSharper disable InconsistentNaming
            XmlSerializer ser_0_1 = new XmlSerializer(typeof(SrmSettingsList));
            XmlSerializer serCurrent = new XmlSerializer(typeof(SrmSettings));
            using (TextReader reader_0_1 = new StringReader(SETTINGS_LIST_0_1))
            using (TextReader readerCurrent = new StringReader(SETTINGS_CURRENT))
            {
                SrmSettings settings_0_1 = ((SrmSettingsList) ser_0_1.Deserialize(reader_0_1))[0];
                SrmSettings settingsCurrent = (SrmSettings) serCurrent.Deserialize(readerCurrent);
                AssertEx.SettingsCloned(settings_0_1, settingsCurrent);
            }
// ReSharper restore InconsistentNaming
        }

        private const string SETTINGS_LIST_0_1 =
            "<SrmSettingsList>\n" +
            "    <ArrayOfSrmSettings xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
            "        xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n" +
            "        <SrmSettings name=\"Default\">\n" +
            "            <peptide_settings>\n" +
            "                <enzyme name=\"LysN promisc\" cut=\"KASR\" no_cut=\"\" sense=\"N\" />\n" +
            "                <digest_settings max_missed_cleavages=\"1\" exclude_ragged_ends=\"true\" />\n" +
            "                <peptide_filter start=\"0\" min_length=\"5\" max_length=\"30\" min_transtions=\"4\"\n" +
            "                    auto_select=\"true\">\n" +
            "                    <peptide_exclusions>\n" +
            "                        <exclusion name=\"Met\" regex=\"[M]\" />\n" +
            "                        <exclusion name=\"NXT/NXS\" regex=\"N.[TS]\" />\n" +
            "                        <exclusion name=\"D Runs\" regex=\"DDDD\" />\n" +
            "                    </peptide_exclusions>\n" +
            "                </peptide_filter>\n" +
            "                <peptide_modifications>\n" +
            "                    <static_modifications>\n" +
            "                        <static_modification name=\"Test2\" aminoacid=\"M\"\n" +
            "                            massdiff_monoisotopic=\"5\" massdiff_average=\"5.1\" />\n" +
            "                        <static_modification name=\"Test3\" aminoacid=\"K\" terminus=\"N\"\n" +
            "                            formula=\"CH3ON2\" />\n" +
            "                    </static_modifications>\n" +
            "                </peptide_modifications>\n" +
            "            </peptide_settings>\n" +
            "            <transition_settings>\n" +
            "                <transition_prediction precursor_mass_type=\"Average\" fragment_mass_type=\"Average\">\n" +
            "                    <predict_collision_energy name=\"ABI\">\n" +
            "                        <regressions>\n" +
            "                            <regression_ce slope=\"0.0431\" intercept=\"4.7556\" charge=\"2\" />\n" +
            "                        </regressions>\n" +
            "                    </predict_collision_energy>\n" +
            // Retention time moved from transition to prediction
            "                    <predict_retention_time name=\"Bovine Standard (100A)\" calculator=\"SSRCalc 3.0 (100A)\"\n" +
            "                        time_window=\"13.6\">\n" +
            "                        <regression_rt slope=\"1.681\" intercept=\"-6.247\" />\n" +
            "                    </predict_retention_time>\n" +
            "                    <predict_declustering_potential slope=\"0.5\" intercept=\"5\" name=\"Test1\" />\n" +
            "                </transition_prediction>\n" +
            "                <transition_filter precursor_charges=\"2,3\" product_charges=\"1,2\"\n" +
            "                    fragment_range_first=\"y3\" fragment_range_last=\"last y-ion - 1\"\n" +
            "                    include_n_prolene=\"true\" include_c_glu_asp=\"true\" auto_select=\"true\" />\n" +
            "                <transition_instrument min_mz=\"52\" max_mz=\"1503\" />\n" +
            "            </transition_settings>\n" +
            "        </SrmSettings>\n" +
            "    </ArrayOfSrmSettings>\n" +
            "</SrmSettingsList>";

        /// <summary>
        /// Test de/serialization of all the other types of lists stored
        /// in user.config.
        /// </summary>
        [TestMethod]
        public void SettingsSerializeListsTest()
        {
            AssertEx.Serialization<EnzymeList>(SETTINGS_ENZYME_LIST, CheckSettingsList);
            AssertEx.Serialization<StaticModList>(SETTINGS_STATIC_MOD_LIST, CheckSettingsList);
            AssertEx.Serialization<HeavyModList>(SETTINGS_HEAVY_MOD_LIST, CheckSettingsList);
            AssertEx.Serialization<PeptideExcludeList>(SETTINGS_EXCLUSIONS_LIST, CheckSettingsList);
            AssertEx.Serialization<CollisionEnergyList>(SETTINGS_CE_LIST, CheckSettingsList);
            AssertEx.Serialization<DeclusterPotentialList>(SETTINGS_DP_LIST, CheckSettingsList);
            AssertEx.Serialization<RetentionTimeList>(SETTINGS_RT_LIST, CheckSettingsList);
        }

        private const string SETTINGS_ENZYME_LIST =
            "<EnzymeList>\n" +
            "    <enzyme name=\"Trypsin\" cut=\"KR\" no_cut=\"P\" sense=\"C\" />\n" +
            "    <enzyme name=\"Trypsin/P\" cut=\"KR\" no_cut=\"\" sense=\"C\" />\n" +
            "    <enzyme name=\"Chymotrypsin\" cut=\"FWYM\" no_cut=\"P\" sense=\"C\" />\n" +
            "    <enzyme name=\"AspN\" cut=\"D\" no_cut=\"\" sense=\"N\" />\n" +
            "</EnzymeList>";

        private const string SETTINGS_STATIC_MOD_LIST =
            "<StaticModList>\n" +
            "    <static_modification name=\"Test1\" aminoacid=\"C\"\n" +
            "        formula=\"C2H3 - ON4\" />\n" +
            "    <static_modification name=\"Test2\" terminus=\"N\" massdiff_monoisotopic=\"5\"\n" +
            "        massdiff_average=\"5.1\" />\n" +
            "    <static_modification name=\"Test3\" aminoacid=\"K\" terminus=\"N\"\n" +
            "        formula=\"CH3ON2\" />\n" +
            "</StaticModList>";

        private const string SETTINGS_HEAVY_MOD_LIST =
            "<HeavyModList>\n" +
            "    <static_modification name=\"Test1\" aminoacid=\"C\"\n" +
            "        formula=\"C2H3 - ON4\" />\n" +
            "    <static_modification name=\"Test2\" terminus=\"N\" massdiff_monoisotopic=\"5\"\n" +
            "        massdiff_average=\"5.1\" />\n" +
            "    <static_modification name=\"Test3\" aminoacid=\"K\" terminus=\"N\"\n" +
            "        formula=\"CH3ON2\" />\n" +
            "</HeavyModList>";

        private const string SETTINGS_EXCLUSIONS_LIST =
            "<PeptideExcludeList>\n" +
            "    <exclusion name=\"Cys\" regex=\"[C]\" />\n" +
            "    <exclusion name=\"Met\" regex=\"[M]\" />\n" +
            "    <exclusion name=\"His\" regex=\"[H]\" />\n" +
            "    <exclusion name=\"NXT/NXS\" regex=\"N.[TS]\" />\n" +
            "    <exclusion name=\"RP/KP\" regex=\"[RK]P\" />\n" +
            "    <exclusion name=\"D Runs\" regex=\"DDDD\" />\n" +
            "</PeptideExcludeList>";

        private const string SETTINGS_CE_LIST =
            "<CollisionEnergyList>\n" +
            "    <predict_collision_energy name=\"Thermo\">\n" +
            "        <regression_ce charge=\"2\" slope=\"0.034\" intercept=\"3.314\" />\n" +
            "        <regression_ce charge=\"3\" slope=\"0.044\" intercept=\"3.314\" />\n" +
            "    </predict_collision_energy>\n" +
            "    <predict_collision_energy name=\"ABI\">\n" +
            "        <regression_ce charge=\"2\" slope=\"0.0431\" intercept=\"4.7556\" />\n" +
            "    </predict_collision_energy>\n" +
            "</CollisionEnergyList>";    

        private const string SETTINGS_DP_LIST =
            "<DeclusterPotentialList>\n" +
            "    <predict_declustering_potential name=\"None\" slope=\"0\" intercept=\"0\" />\n" +
            "    <predict_declustering_potential name=\"ABI\" slope=\"0.0729\" intercept=\"31.117\" />\n" +
            "    <predict_declustering_potential name=\"Test1\" slope=\"0.5\" intercept=\"5\" />\n" +
            "</DeclusterPotentialList>";

        private const string SETTINGS_RT_LIST =
            "<RetentionTimeList>\n" +
            "    <predict_retention_time name=\"None\" time_window=\"0\">\n" +
            "        <regression_rt slope=\"0\" intercept=\"0\" />\n" +
            "    </predict_retention_time>\n" +
            "    <predict_retention_time name=\"Bovine Standard (100A)\" calculator=\"SSRCalc 3.0 (100A)\"\n" +
            "        time_window=\"13.6\">\n" +
            "        <regression_rt slope=\"1.681\" intercept=\"-6.247\" />\n" +
            "    </predict_retention_time>\n" +
            "</RetentionTimeList>";

        /// <summary>
        /// Test XML deserialization where major parts are missing.
        /// </summary>
        [TestMethod]
        public void SettingsSerializeStubTest()
        {
            XmlSerializer ser = new XmlSerializer(typeof(SrmSettings));
            using (TextReader reader = new StringReader(XML_DIRECTIVE + SETTINGS_STUBS))
            {
                var target = SrmSettingsList.GetDefault();
                var copy = (SrmSettings) ser.Deserialize(reader);
                Assert.AreSame(target.PeptideSettings.Enzyme, copy.PeptideSettings.Enzyme);
                Assert.AreSame(target.PeptideSettings.DigestSettings, copy.PeptideSettings.DigestSettings);
                AssertEx.Cloned(target.PeptideSettings.Prediction, copy.PeptideSettings.Prediction);
                Assert.AreSame(target.PeptideSettings.Filter, copy.PeptideSettings.Filter);
                Assert.AreSame(target.PeptideSettings.Libraries, copy.PeptideSettings.Libraries);
                Assert.AreSame(target.PeptideSettings.Modifications, copy.PeptideSettings.Modifications);
                AssertEx.Cloned(target.PeptideSettings, copy.PeptideSettings);
                Assert.AreSame(target.TransitionSettings.Prediction, copy.TransitionSettings.Prediction);
                Assert.AreSame(target.TransitionSettings.Filter, copy.TransitionSettings.Filter);
                Assert.AreSame(target.TransitionSettings.Libraries, copy.TransitionSettings.Libraries);
                Assert.AreSame(target.TransitionSettings.Instrument, copy.TransitionSettings.Instrument);
                AssertEx.Cloned(target.TransitionSettings, copy.TransitionSettings);
                AssertEx.Cloned(target, copy);
            }                        
        }

        // This string should deserialize successfully to the default SRM settings.
        private const string SETTINGS_STUBS =
            "<settings_summary name=\"Default\">\n" +
            "    <peptide_settings>\n" +
            "        <peptide_prediction/>\n" +
            "    </peptide_settings>\n" +
            "    <transition_settings/>\n" +
            "</settings_summary>";

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="Enzyme"/>.
        /// </summary>
        [TestMethod]
        public void SerializeEnzymeTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<Enzyme>("<enzyme name=\"Validate (1)\" cut=\"M\" no_cut=\"P\" sense=\"C\" />");
            AssertEx.DeserializeNoError<Enzyme>("<enzyme name=\"Validate (2)\" cut=\"M\" sense=\"N\" />");
            AssertEx.DeserializeNoError<Enzyme>("<enzyme name=\"Validate (3)\" cut=\"ACDEFGHIKLMNPQRSTVWY\" />");

            // Missing parameters
            AssertEx.DeserializeError<Enzyme>("<enzyme/>");
            // No name
            AssertEx.DeserializeError<Enzyme>("<enzyme cut=\"KR\" no_cut=\"P\" sense=\"C\" />");
            // No cleavage
            AssertEx.DeserializeError<Enzyme>("<enzyme name=\"Trypsin\" cut=\"\" no_cut=\"P\" sense=\"C\" />");
            // Bad cleavage
            AssertEx.DeserializeError<Enzyme>("<enzyme name=\"Trypsin\" cut=\"X\" no_cut=\"P\" sense=\"C\" />");
            AssertEx.DeserializeError<Enzyme>("<enzyme name=\"Trypsin\" cut=\"MKRM\" no_cut=\"P\" sense=\"C\" />");
            // Bad restrict
            AssertEx.DeserializeError<Enzyme>("<enzyme name=\"Trypsin\" cut=\"KR\" no_cut=\"+\" sense=\"C\" />");
            AssertEx.DeserializeError<Enzyme>("<enzyme name=\"Trypsin\" cut=\"KR\" no_cut=\"AMRGR\" sense=\"C\" />");
            // Bad sense
            AssertEx.DeserializeError<Enzyme>("<enzyme name=\"Trypsin\" cut=\"KR\" no_cut=\"P\" sense=\"X\" />");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="DigestSettings"/>.
        /// </summary>
        [TestMethod]
        public void SerializeDigestTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<DigestSettings>("<digest_settings max_missed_cleavages=\"0\" exclude_ragged_ends=\"true\" />");
            AssertEx.DeserializeNoError<DigestSettings>("<digest_settings max_missed_cleavages=\"9\" exclude_ragged_ends=\"false\" />");
            AssertEx.DeserializeNoError<DigestSettings>("<digest_settings/>");

            // Errors
            AssertEx.DeserializeError<DigestSettings>("<digest_settings max_missed_cleavages=\"10\" exclude_ragged_ends=\"true\" />");
            AssertEx.DeserializeError<DigestSettings>("<digest_settings max_missed_cleavages=\"-1\" exclude_ragged_ends=\"true\" />");
            AssertEx.DeserializeError<DigestSettings>("<digest_settings max_missed_cleavages=\"0\" exclude_ragged_ends=\"yes\" />");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="DigestSettings"/>.
        /// </summary>
        [TestMethod]
        public void SerializePeptidePredictionTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<PeptidePrediction>("<peptide_prediction />");
            AssertEx.DeserializeNoError<PeptidePrediction>("<peptide_prediction use_measured_rts=\"false\" />");
            AssertEx.DeserializeNoError<PeptidePrediction>("<peptide_prediction use_measured_rts=\"true\" />");
            AssertEx.DeserializeNoError<PeptidePrediction>("<peptide_prediction use_measured_rts=\"true\" measured_rt_window=\"2.0\"/>");
            AssertEx.DeserializeNoError<PeptidePrediction>("<peptide_prediction use_measured_rts=\"false\" measured_rt_window=\"2.0\"/>");
            AssertEx.DeserializeNoError<PeptidePrediction>("<peptide_prediction measured_rt_window=\"5.0\"/>");

            // Errors (out of range)
            AssertEx.DeserializeError<PeptidePrediction>("<peptide_prediction measured_rt_window=\"0.1\"/>");
            AssertEx.DeserializeError<PeptidePrediction>("<peptide_prediction measured_rt_window=\"60.0\"/>");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="PeptideFilter"/>.
        /// </summary>
        [TestMethod]
        public void SerializePeptideFilterTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<PeptideFilter>("<peptide_filter min_length=\"2\" max_length=\"200\" min_transtions=\"1\"/>");
            AssertEx.DeserializeNoError<PeptideFilter>("<peptide_filter start=\"0\" min_length=\"100\" max_length=\"100\" min_transtions=\"20\" auto_select=\"false\"/>");
            AssertEx.DeserializeNoError<PeptideFilter>("<peptide_filter start=\"100\" min_length=\"2\" max_length=\"5\" auto_select=\"true\"/>");
            AssertEx.DeserializeNoError<PeptideFilter>("<peptide_filter start=\"25\" min_length=\"8\" max_length=\"25\" auto_select=\"true\"><peptide_exclusions/></peptide_filter>");

            // Missing parameters
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter/>");
            // min_length range
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter min_length=\"1\" max_length=\"30\"/>");
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter min_length=\"500\" max_length=\"30\"/>");
            // max_length range
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter min_length=\"10\" max_length=\"8\"/>");
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter min_length=\"4\" max_length=\"4\"/>");
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter min_length=\"8\" max_length=\"500\"/>");
            // start range
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter start=\"-1\" min_length=\"8\" max_length=\"25\"/>");
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter start=\"50000\" min_length=\"8\" max_length=\"25\"/>");
            // bad exclusions
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter start=\"25\" min_length=\"8\" max_length=\"25\">" +
                            "<peptide_exclusions><exclusion name=\"Noex\"/></peptide_exclusions></peptide_exclusions></peptide_filter>");
            AssertEx.DeserializeError<PeptideFilter>("<peptide_filter start=\"25\" min_length=\"8\" max_length=\"25\">" +
                            "<peptide_exclusions><exclusion regex=\"PX\"/></peptide_exclusions></peptide_exclusions></peptide_filter");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="PeptideModifications"/>.
        /// </summary>
        [TestMethod]
        public void SerializePeptideModificationsTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<PeptideModifications>("<peptide_modifications><static_modifications/></peptide_modifications>");
            AssertEx.DeserializeNoError<PeptideModifications>("<peptide_modifications/>");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="PeptideModifications"/>.
        /// </summary>
        [TestMethod]
        public void SerializeStaticModTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<StaticMod>("<static_modification name=\"Mod\" aminoacid=\"R\" terminus=\"C\" formula=\"C2H3ON15PS\" />");
            AssertEx.DeserializeNoError<StaticMod>("<static_modification name=\"Mod\" terminus=\"N\" formula=\"-ON4\" />");
            AssertEx.DeserializeNoError<StaticMod>("<static_modification name=\"Mod\" aminoacid=\"P\" formula=\"C23 - O N P14\" />");
            AssertEx.DeserializeNoError<StaticMod>("<static_modification name=\"Mod\" aminoacid=\"P\" massdiff_monoisotopic=\"5\"\n" +
                    " massdiff_average=\"5.1\" />");
            AssertEx.DeserializeNoError<StaticMod>("<static_modification name=\"Mod\" formula=\"C23N\" />");
            AssertEx.DeserializeNoError<StaticMod>("<static_modification name=\"15N\" label_15N=\"true\" />");
            AssertEx.DeserializeNoError<StaticMod>("<static_modification name=\"Heavy K\" aminoacid=\"K\" label_13C=\"true\" label_15N=\"true\" label_18O=\"true\"  label_2H=\"true\"/>");

            // Missing parameters
            AssertEx.DeserializeError<StaticMod>("<static_modification />");
            // Bad amino acid
            AssertEx.DeserializeError<StaticMod>("<static_modification name=\"Mod\" aminoacid=\"X\" formula=\"C23N\" />");
            AssertEx.DeserializeError<StaticMod>("<static_modification name=\"Mod\" aminoacid=\"KR\" formula=\"C23N\" />");
            // Bad terminus
            AssertEx.DeserializeError<StaticMod>("<static_modification name=\"Mod\" terminus=\"X\" formula=\"C23N\" />");
            // Bad formula
            AssertEx.DeserializeError<StaticMod, ArgumentException>("<static_modification name=\"Mod\" aminoacid=\"K\" formula=\"C23NAu2\" />");
            // Terminal label without amino acid
            AssertEx.DeserializeError<StaticMod>("<static_modification name=\"15N\" terminus=\"C\" label_13C=\"true\"/>");
            // Formula and labeled atoms
            AssertEx.DeserializeError<StaticMod>("<static_modification name=\"15N\" label_15N=\"true\" formula=\"C23N\" />");
            // Missing formula and masses
            AssertEx.DeserializeError<StaticMod>("<static_modification name=\"Mod\" aminoacid=\"R\" />");
            AssertEx.DeserializeError<StaticMod, ArgumentException>("<static_modification name=\"Mod\" aminoacid=\"R\" formula=\"\" />");
            // Both formula and masses
            AssertEx.DeserializeError<StaticMod>("<static_modification name=\"Mod\" aminoacid=\"P\" formula=\"C23N\" massdiff_monoisotopic=\"5\"\n" +
                    " massdiff_average=\"5.1\" />");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="TransitionPrediction"/>.
        /// </summary>
        [TestMethod]
        public void SerializeTransitionPredictionTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<TransitionPrediction>("<transition_prediction>" +
                "<predict_collision_energy name=\"Pass\">" +
                "<regressions><regression_ce slope=\"0.1\" intercept=\"4.7\" charge=\"2\" /></regressions>" +
                "</predict_collision_energy></transition_prediction>");

            // Bad mass type
            AssertEx.DeserializeError<TransitionPrediction>("<transition_prediction precursor_mass_type=\"Bad\">" +
                "<predict_collision_energy name=\"Fail\">" +
                "<regressions><regression_ce slope=\"0.1\" intercept=\"4.7\" charge=\"2\" /></regressions>" +
                "</predict_collision_energy></transition_prediction>");
            // No collision energy regression
            AssertEx.DeserializeError<TransitionPrediction>("<transition_prediction/>");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="CollisionEnergyRegression"/>.
        /// </summary>
        [TestMethod]
        public void SerializeCollisionEnergyTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<CollisionEnergyRegression>("<predict_collision_energy name=\"Pass\">" +
                "<regression_ce slope=\"0.1\" intercept=\"4.7\" charge=\"2\" />" +
                "</predict_collision_energy>");
            AssertEx.DeserializeNoError<CollisionEnergyRegression>("<predict_collision_energy name=\"Pass\">" +
                "<regression_ce charge=\"1\" /><regression_ce charge=\"2\" /><regression_ce charge=\"3\" /><regression_ce charge=\"4\" />" +
                "</predict_collision_energy>");
            // v0.1 format
            AssertEx.DeserializeNoError<CollisionEnergyRegression>("<predict_collision_energy name=\"Pass\">" +
                "<regressions><regression_ce /></regressions>" +
                "</predict_collision_energy>");

            // No regressions
            AssertEx.DeserializeError<CollisionEnergyRegression>("<predict_collision_energy name=\"Fail\" />");
            // Repeated charge
            AssertEx.DeserializeError<CollisionEnergyRegression>("<predict_collision_energy name=\"Fail\">" +
                    "<regression_ce slope=\"0.1\" intercept=\"4.7\" charge=\"2\" />" + 
                    "<regression_ce slope=\"0.1\" intercept=\"4.7\" charge=\"2\" />" +
                "</predict_collision_energy>");
        }

        [TestMethod]
        public void SerializeCollisionEnergyListTest()
        {
            XmlSerializer ser = new XmlSerializer(typeof(CollisionEnergyList));
            using (TextReader reader = new StringReader(
                "<CollisionEnergyList>\n" +
                "    <predict_collision_energy name=\"Thermo\">\n" +
                "        <regression_ce charge=\"2\" slope=\"0.034\" intercept=\"3.314\" />\n" +
                "        <regression_ce charge=\"3\" slope=\"0.044\" intercept=\"3.314\" />\n" +
                "    </predict_collision_energy>\n" +
                "    <predict_collision_energy name=\"ABI\">\n" +
                "        <regression_ce charge=\"2\" slope=\"0.0431\" intercept=\"4.7556\" />\n" +
                "    </predict_collision_energy>\n" +
                "</CollisionEnergyList>"))
            {
                var listCE = (CollisionEnergyList) ser.Deserialize(reader);

                Assert.AreEqual(5, listCE.Count);
                Assert.AreEqual(1, listCE.RevisionIndexCurrent);

                int i = 0;
                foreach (var regressionCE in listCE.GetDefaults(1))
                {
                    // Check the first 3 items in the defaults, which should be new
                    if (i++ >= 3)
                        break;
                    CollisionEnergyRegression regressionTmp;
                    Assert.IsTrue(listCE.TryGetValue(regressionCE.GetKey(), out regressionTmp));
                }
            }
        }        

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="DeclusteringPotentialRegression"/>.
        /// </summary>
        [TestMethod]
        public void SerializeDeclusteringPotentialTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<DeclusteringPotentialRegression>("<predict_declustering_potential name=\"Pass\"" +
                " slope=\"0.1\" intercept=\"4.7\" />");
            AssertEx.DeserializeNoError<DeclusteringPotentialRegression>("<predict_declustering_potential name=\"Pass\" />");

            // No name
            AssertEx.DeserializeError<DeclusteringPotentialRegression>("<predict_declustering_potential" +
                " slope=\"0.1\" intercept=\"4.7\" />");
            // Non-numeric parameter
            AssertEx.DeserializeError<DeclusteringPotentialRegression>("<predict_declustering_potential name=\"Pass\"" +
                " slope=\"X\" intercept=\"4.7\" />");
        }        

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="TransitionFilter"/>.
        /// </summary>
        [TestMethod]
        public void SerializeTransitionFilterTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<TransitionFilter>("<transition_filter precursor_charges=\"2\" product_charges=\"1\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" " +
                "include_n_prolene=\"true\" include_c_glu_asp=\"true\" auto_select=\"true\" />");
            AssertEx.DeserializeNoError<TransitionFilter>("<transition_filter precursor_charges=\"1,2,3,4,5\" product_charges=\"1,2\" " +
                "fragment_range_first=\"(m/z > precursor) - 2\" fragment_range_last=\"start + 4\" " +
                "include_n_prolene=\"false\" include_c_glu_asp=\"false\" auto_select=\"false\" />");
            AssertEx.DeserializeNoError<TransitionFilter>("<transition_filter precursor_charges=\"2\" product_charges=\"1\" " +
                "fragment_range_first=\"m/z > precursor\" fragment_range_last=\"last y-ion - 3\" />");

            // Bad charges
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"0\" product_charges=\"1\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"2\" product_charges=\"0\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"2,2\" product_charges=\"1\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"3\" product_charges=\"4\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"6\" product_charges=\"2\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"\" product_charges=\"1\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last y-ion\" />");
            // Bad fragments
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"2\" product_charges=\"1\" " +
                "fragment_range_first=\"b10\" fragment_range_last=\"last y-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"2\" product_charges=\"1\" " +
                "fragment_range_first=\"y1\" fragment_range_last=\"last z-ion\" />");
            AssertEx.DeserializeError<TransitionFilter>("<transition_filter precursor_charges=\"2\" product_charges=\"1\" />");
        }

        /// <summary>
        /// Test error handling in XML deserialization of <see cref="TransitionInstrument"/>.
        /// </summary>
        [TestMethod]
        public void SerializeTransitionInstrumentTest()
        {
            // Valid first
            AssertEx.DeserializeNoError<TransitionInstrument>("<transition_instrument min_mz=\"52\" max_mz=\"1503\" />");
            AssertEx.DeserializeNoError<TransitionInstrument>("<transition_instrument min_mz=\"10\" max_mz=\"5000\" />");
            AssertEx.DeserializeNoError<TransitionInstrument>("<transition_instrument min_mz=\"10\" max_mz=\"5000\" mz_match_tolerance=\"0.4\"/>");
            AssertEx.DeserializeNoError<TransitionInstrument>("<transition_instrument min_mz=\"10\" max_mz=\"5000\" mz_match_tolerance=\"0.001\"/>");

            // Empty element
            AssertEx.DeserializeError<TransitionInstrument>("<transition_instrument />");
            // Out of range values
            AssertEx.DeserializeError<TransitionInstrument>("<transition_instrument min_mz=\"-1\" max_mz=\"1503\" />");
            AssertEx.DeserializeError<TransitionInstrument>("<transition_instrument min_mz=\"52\" max_mz=\"100\" />");
            AssertEx.DeserializeError<TransitionInstrument>("<transition_instrument min_mz=\"10\" max_mz=\"5000\" mz_match_tolerance=\"0\"/>");
            AssertEx.DeserializeError<TransitionInstrument>("<transition_instrument min_mz=\"10\" max_mz=\"5000\" mz_match_tolerance=\"0.65\"/>");
        }

        private static void CheckSettingsList<TItem>(SettingsList<TItem> target, SettingsList<TItem> copy)
            where TItem : IKeyContainer<string>, IXmlSerializable
        {
            Assert.AreEqual(target.Count, copy.Count);
            for (int i = 0; i < target.Count; i++)
                AssertEx.Cloned(target[i], copy[i]);
        }
    }
}