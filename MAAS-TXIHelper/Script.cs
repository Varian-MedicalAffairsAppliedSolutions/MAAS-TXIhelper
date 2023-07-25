using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Views;
using System.Net.NetworkInformation;
using System.IO;
using JR.Utils.GUI.Forms;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using ViewModels;
using MAAS_TXIHelper.Models;
using System.Globalization;
using Newtonsoft.Json;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
// TODO: Uncomment the following line if the script requires write access.
//15.x or later:
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
        string EULA_TEXT = @"""
            VARIAN LIMITED USE SOFTWARE LICENSE AGREEMENT

            This Limited Use Software License Agreement (the ""Agreement"") is a legal agreement between you , the
            user (“You”), and Varian Medical Systems, Inc. (""Varian""). By downloading or otherwise accessing the
            software material, which includes source code (the ""Source Code"") and related software tools (collectively,
            the ""Software""), You are agreeing to be bound by the terms of this Agreement. If You are entering into this
            Agreement on behalf of an institution or company, You represent and warrant that You are authorized to do
            so. If You do not agree to the terms of this Agreement, You may not use the Software and must immediately
            destroy any Software You may have downloaded or copied.

            SOFTWARE LICENSE

            1. Grant of License. Varian grants to You a non-transferable, non-sublicensable license to use
            the Software solely as provided in Section 2 (Permitted Uses) below. Access to the Software will be
            facilitated through a source code repository provided by Varian.

            2. Permitted Uses. You may download, compile and use the Software, You may (but are not required to do
            so) suggest to Varian improvements or otherwise provide feedback to Varian with respect to the
            Software. You may modify the Software solely in support of such use, and You may upload such
            modified Software to Varian’s source code repository. Any derivation of the Software (including compiled
            binaries) must display prominently the terms and conditions of this Agreement in the interactive user
            interface, such that use of the Software cannot continue until the user has acknowledged having read
            this Agreement via click-through.

            3. Publications. Solely in connection with your use of the Software as permitted under this Agreement, You
            may make reference to this Software in connection with such use in academic research publications
            after notifying an authorized representative of Varian in writing in each instance. Notwithstanding the
            foregoing, You may not make reference to the Software in any way that may indicate or imply any
            approval or endorsement by Varian of the results of any use of the Software by You.

            4. Prohibited Uses. Under no circumstances are You permitted, allowed or authorized to distribute the
            Software or any modifications to the Software for any purpose, including, but not limited to, renting,
            selling, or leasing the Software or any modifications to the Software, for free or otherwise. You may not
            disclose the Software to any third party without the prior express written consent of an authorized
            representative of Varian. You may not reproduce, copy or disclose to others, in whole or in any part, the
            Software or modifications to the Software, except within Your own institution or company, as applicable,
            to facilitate Your permitted use of the Software. You agree that the Software will not be shipped,
            transferred or exported into any country in violation of the U.S. Export Administration Act (or any other
            law governing such matters) and that You will not utilize, in any other manner, the Software in
            violation of any applicable law.

            5. Intellectual Property Rights. All intellectual property rights in the Software and any modifications to the
            Software are owned solely and exclusively by Varian, and You shall have no ownership or other
            proprietary interest in the Software or any modifications. You hereby transfer and assign to Varian all
            right, title and interest in any such modifications to the Software that you may have made or contributed.
            You hereby waive any and all moral rights that you may have with respect to such modifications, and
            hereby waive any rights of attribution relating to any modifications of the Software. You acknowledge
            that Varian will have the sole right to commercialize and otherwise use, whether directly or through third
            parties, any modifications to the Software that you provide to Varian’s repository. Varian may make any
            use it determines to be appropriate with respect to any feedback, suggestions or other communications
            that You provide with respect to the Software or any modifications.

            6. No Support Obligations. Varian is under no obligation to provide any support or technical assistance in
            connection with the Software or any modifications. Any such support or technical assistance is entirely
            discretionary on the part of Varian, and may be discontinued at any time without liability.

            7. NO WARRANTIES. THE SOFTWARE AND ANY SUPPORT PROVIDED BY VARIAN ARE PROVIDED
            “AS IS” AND “WITH ALL FAULTS.” VARIAN DISCLAIMS ALL WARRANTIES, BOTH EXPRESS AND
            IMPLIED, INCLUDING BUT NOT LIMITED TO IMPLIED WARRANTIES OF MERCHANTABILITY,
            FITNESS FOR A PARTICULAR PURPOSE, AND NON-INFRINGEMENT WITH RESPECT TO THE
            SOFTWARE AND ANY SUPPORT. VARIAN DOES NOT WARRANT THAT THE OPERATION OF THE
            SOFTWARE WILL BE UNINTERRUPTED, ERROR FREE OR MEET YOUR SPECIFIC
            REQUIREMENTS OR INTENDED USE. THE AGENTS AND EMPLOYEES OF VARIAN ARE NOT
            AUTHORIZED TO MAKE MODIFICATIONS TO THIS PROVISION, OR PROVIDE ADDITIONAL
            WARRANTIES ON BEHALF OF VARIAN.

            8. No Regulatory Clearance. The Software is not cleared or approved for use by any regulatory body in any
            jurisdiction.

            9. Termination. You may terminate this Agreement, and the right to use the Software, at any time upon
            written notice to Varian. Varian may terminate this Agreement, and the right to use the Software, at any
            time upon notice to You in the event that Varian determines that you are not using the Software in
            accordance with this Agreement or have otherwise breached any provision of this Agreement. The
            Software, together with any modifications to it or any permitted archive copy thereof, shall be destroyed
            when no longer used in accordance with this Agreement, or when the right to use the Software is
            terminated.

            10. Limitation of Liability. IN NO EVENT SHALL VARIAN BE LIABLE FOR LOSS OF DATA, LOSS OF
            PROFITS, LOST SAVINGS, SPECIAL, INCIDENTAL, CONSEQUENTIAL, INDIRECT OR
            OTHER SIMILAR DAMAGES ARISING FROM BREACH OF WARRANTY, BREACH OF
            CONTRACT, NEGLIGENCE, OR OTHER LEGAL THEORY EVEN IF VARIAN OR ITS AGENT HAS
            BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES, OR FOR ANY CLAIM BY ANY OTHER
            PARTY.

            11. Indemnification. You will defend, indemnify and hold harmless Varian, its affiliates and their respective
            officers, directors, employees, sublicensees, contractors, users and agents from any and all claims,
            losses, liabilities, damages, expenses and costs (including attorneys’ fees and court costs) arising out of
            any third-party claims related to or arising from your use of the Software or any modifications to the
            Software.

            12. Assignment. You may not assign any of Your rights or obligations under this Agreement without the
            written consent of Varian.

            13. Governing Law. This Agreement will be governed and construed under the laws of the State of California
            and the United States of America without regard to conflicts of law provisions. The parties agree to the
            exclusive jurisdiction of the state and federal courts located in Santa Clara County, California with
            respect to any disputes under or relating to this Agreement.

            14. Entire Agreement. This Agreement is the entire agreement of the parties as to the subject matter and
            supersedes all prior written and oral agreements and understandings relating to same. The Agreement
            may only be modified or amended in a writing signed by the parties that makes specific reference to the
            Agreement and the provision the parties intend to modify or amend.""";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context)
        {
            //var mw = new MahApps.Metro.Controls.MetroWindow();

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var noexp_path = Path.Combine(path, "NOEXPIRE");
            bool foundNoExpire = File.Exists(noexp_path);

            // search for json config in current dir
            var json_path = Path.Combine(path, "config.json");
            if (!File.Exists(json_path)) { throw new Exception($"Could not locate json path {json_path}"); }

            // Test
            // Create serialized verion of settings
            /*
            var settings = new SettingsClass();
            settings.Debug = false;
            settings.EULAAgreed = false;
            settings.Validated= false;
            settings.ExpirationDate = DateTime.Parse("1/1/2024");
            File.WriteAllText(Path.Combine(path, "config.json"), JsonConvert.SerializeObject(settings));*/

            var settings = JsonConvert.DeserializeObject<SettingsClass>(File.ReadAllText(json_path));

            if (context.Patient == null || context.PlanSetup == null)
            {
                MessageBox.Show("No active plan selected - exiting.");
                return;
            }

            //var asmCa = typeof(StartupCore).Assembly.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(AssemblyExpirationDate));
            //if (configUpdate != null && DateTime.TryParse(asmCa.ConstructorArguments.FirstOrDefault().Value as string, provider, DateTimeStyles.None, out endDate) && eulaValue == "true")

            var asmCa = Assembly.GetExecutingAssembly().CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(AssemblyExpirationDate));
            DateTime exp;
            var provider = new CultureInfo("en-US");
            DateTime.TryParse(asmCa.ConstructorArguments.FirstOrDefault().Value as string, provider, DateTimeStyles.None, out exp);

            //MessageBox.Show(exp.ToString());

            // Check exp date
            //DateTime exp = settings.ExpirationDate;
            

            if (exp < DateTime.Now && !foundNoExpire)
            {
                MessageBox.Show("Application has expired. Newer builds with future expiration dates can be found here: https://github.com/Varian-Innovation-Center/MAAS-MAAS_TXIHelper");
                return;
            }
            

            // Initial EULA agreement
            if (!settings.EULAAgreed)
            {
                var res = FlexibleMessageBox.Show(EULA_TEXT, "EULA Agreement", MessageBoxButtons.YesNo);
                if (res == DialogResult.No)
                {
                    return;
                }
                else
                {
                    settings.EULAAgreed = true;
                    File.WriteAllText(json_path, JsonConvert.SerializeObject(settings));
                }
            }

            // Display opening msg
            string msg = $"The current MAAS_TXIHelper application is provided AS IS as a non-clinical, research only tool in evaluation only. The current " +
            $"application will only be available until {exp.Date} after which the application will be unavailable. " +
            "By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support\n\n" +
            "Newer builds with future expiration dates can be found here: https://github.com/Varian-Innovation-Center/MAAS-MAAS_TXIHelper\n\n" +
            "See the FAQ for more information on how to remove this pop-up and expiration";

            string msg2 = $"Application will only be available until {exp.Date} after which the application will be unavailable. " +
            "By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support\n\n" +
            "Newer builds with future expiration dates can be found here: https://github.com/Varian-Innovation-Center/MAAS-MAAS_TXIHelper\n\n" +
            "See the FAQ for more information on how to remove this pop-up and expiration";

            
            // Test
            //MessageBox.Show($"Noexpire found: {foundNoExpire} looked in : {noexp_path}");

            if (!foundNoExpire)
            {
                if (!settings.Validated)
                {
                    var res = MessageBox.Show(msg, "Agreement  ", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                else if (settings.Validated)
                {
                    var res = MessageBox.Show(msg2, "Agreement  ", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            }

            var mainWindow = new MainWindow(context, new MainViewModel(context, settings.Validated));
            mainWindow.ShowDialog();
        }
    
  }
}
