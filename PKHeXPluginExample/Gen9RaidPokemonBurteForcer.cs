using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using PKHeX.Core;

namespace Gen9PokemonBurteForcer
{
    public class Gen9RaidPokemonBurteForcer : IPlugin
    {
        public string Name => nameof(Gen9RaidPokemonBurteForcer);
        public int Priority => 170; // Loading order, lowest is first.

        // Initialized on plugin load
        public ISaveFileProvider SaveFileEditor { get; private set; } = null!;
        public IEncounterConvertible EncounterConvertible { get; private set; } = null!;
        public ITrainerInfo TrainerInfo { get; private set; } = null!;
        public IPKMView PKMEditor { get; private set; } = null!;
        public static PK9 pk { get; private set; } = null!; 
        public static bool found = false;

        // public int HP, ATT, DEF, SPA, SPD, SPE;
        //public int nature;
        //public int species;
        private Form formui = new Form();
        TextBox HP = new TextBox();
        TextBox ATK = new TextBox();
        TextBox DEF = new TextBox();
        TextBox SPA = new TextBox();
        TextBox SPD = new TextBox();
        TextBox SPE = new TextBox();

        CheckBox HPc  = new CheckBox();
        CheckBox ATKc = new CheckBox();
        CheckBox DEFc = new CheckBox();
        CheckBox SPAc = new CheckBox();
        CheckBox SPDc = new CheckBox();
        CheckBox SPEc = new CheckBox();

        TextBox Nature = new TextBox();
        TextBox species = new TextBox();

        public void Initialize(params object[] args)
        {
            Console.WriteLine($"Loading {Name}...");
            SaveFileEditor = (ISaveFileProvider)Array.Find(args, z => z is ISaveFileProvider);
            EncounterConvertible = (IEncounterConvertible)Array.Find(args, z => z is IEncounterConvertible);
            TrainerInfo = (ITrainerInfo)Array.Find(args, z => z is ITrainerInfo);
            PKMEditor = (IPKMView)Array.Find(args, z => z is IPKMView);

            var menu = (ToolStrip)Array.Find(args, z => z is ToolStrip);
            
            LoadMenuStrip(menu);
        }

        private void LoadMenuStrip(ToolStrip menuStrip)
        {
            var items = menuStrip.Items;
            if (!(items.Find("Menu_Tools", false)[0] is ToolStripDropDownItem tools))
                throw new ArgumentException(nameof(menuStrip));
            AddPluginControl(tools);
        }

        private void AddPluginControl(ToolStripDropDownItem tools)
        {
            var ctrl = new ToolStripMenuItem(Name);
            tools.DropDownItems.Add(ctrl);

            var c2 = new ToolStripMenuItem($"{Name} settings");
            c2.Click += (s, e) => generateForm();
            // var c3 = new ToolStripMenuItem($"{Name} show message");
            // c3.Click += (s, e) => MessageBox.Show("Hello!");
            var c4 = new ToolStripMenuItem($"{Name} modify current SaveFile");
            c4.Click += (s, e) => ModifySaveFile();
            ctrl.DropDownItems.Add(c2);
            // ctrl.DropDownItems.Add(c3);
            // ctrl.DropDownItems.Add(c4);
            Console.WriteLine($"{Name} added menu items.");
        }

       

        private void ModifySaveFile()
        {

            var sav = SaveFileEditor.SAV;

            Thread thread = new Thread(Bruteforcer);
            thread.Start();

        }

        private void Bruteforcer()
        {
            var sav = SaveFileEditor.SAV;
            var tr = TrainerInfo;
            EncounterCriteria ec = new EncounterCriteria();
            var collection = SearchDatabase();
            IEncounterable es = new EncounterStatic9(sav.Version) { Species = ushort.Parse(species.Text), Location = 30024 };
            foreach (IEncounterable einfo in collection)
            {
                if (einfo.Species == ushort.Parse(species.Text))
                {
                    es = einfo;
                    break;
                }
               
            }

            pk = (PK9)es.ConvertToPKM(tr, ec);
           
            while (!PKMcheck(pk))
            {
                pk = (PK9)es.ConvertToPKM(tr, ec);
            }
            sav.ModifyBoxes(ModifyPKM, 0, 0);
            SaveFileEditor.ReloadSlots();
        }

        private bool PKMcheck(PKM target)
        {
            if (target == null)
                return false;
            if (target.IV_ATK != Int32.Parse(ATK.Text) && ATKc.Checked)
                return false;
            if (target.IV_DEF != Int32.Parse(DEF.Text) && DEFc.Checked)
                return false;
            if (target.IV_SPA != Int32.Parse(SPA.Text) && SPAc.Checked)
                return false;
            if (target.IV_SPD != Int32.Parse(SPD.Text) && SPDc.Checked)
                return false;
            if (target.IV_HP != Int32.Parse(HP.Text) && HPc.Checked)
                return false;
            if (target.IV_SPE != Int32.Parse(SPE.Text) && SPEc.Checked)
                return false;
            if (target.Nature != Int32.Parse(Nature.Text) && Int32.Parse(Nature.Text) != 25)
                return false;
            return true;

        }

        public static void ModifyPKM(PKM pkm)
        {
            
            // This will copy global pk into pkm 
            Span<int> span = new Span<int>(new int[6]);
            pk.GetIVs(span);
            pkm.SetIVs(span);
            pkm.Species = pk.Species;
            ((PK9)pkm).SetTeraType(pk.TeraType);
            pkm.EncryptionConstant = pk.EncryptionConstant;
            pkm.SetGender(pk.Gender);
            pkm.SetAbility(pk.Ability);
            ((PK9)pkm).Checksum = pk.Checksum;
            ((PK9)pkm).Nature= pk.Nature;
            //((PK9)pkm).SetNature(pk.Nature);
            pkm.StatNature= pk.StatNature;
            pkm.EXP = pk.EXP;
            pkm.CurrentLevel = pk.CurrentLevel;
            pkm.Met_Location= pk.Met_Location;
            pkm.Nickname = pk.Nickname;
            pkm.Met_Location = pk.Met_Location;
            pkm.MetDate = pk.MetDate;
            pkm.Met_Level= pk.Met_Level;    
            pkm.Met_Month= pk.Met_Month;
            pkm.Met_Year= pk.Met_Year;
            pkm.CurrentFriendship= pk.CurrentFriendship;
            pkm.Move1= pk.Move1;
            pkm.Move2= pk.Move2;
            pkm.Move3= pk.Move3;
            pkm.Move4= pk.Move4;
            pkm.OT_Name= pk.OT_Name;
            pkm.OT_Gender= pk.OT_Gender;
            pkm.Ball= pk.Ball;
            pkm.Move1_PP= pk.Move1_PP;
            pkm.Move2_PP= pk.Move2_PP;
            pkm.Move3_PP= pk.Move3_PP;
            pkm.Move4_PP= pk.Move4_PP;
            pkm.SID= pk.SID;
            pkm.PID= pk.PID;
            pkm.OT_Friendship= pk.OT_Friendship;
            ((PK9)pkm).OT_Gender = pk.OT_Gender;
            pkm.HT_Name= pk.HT_Name;
            pkm.HT_Gender= pk.HT_Gender;
            
            pkm.AbilityNumber= pk.AbilityNumber;
            pkm.Enjoyment= pk.Enjoyment;
            ((PK9)pkm).Obedience_Level = pk.Obedience_Level; 
            ((PK9)pkm).HeightScalar = pk.HeightScalar;
            ((PK9)pkm).WeightScalar= pk.WeightScalar;
            ((PK9)pkm).Scale= pk.Scale;
            ((PK9)pkm).TeraTypeOriginal= pk.TeraTypeOriginal;
            ((PK9)pkm).TeraTypeOverride= pk.TeraTypeOverride;
            ((PK9)pkm).Version= pk.Version;
            ((PK9)pkm).StatNature= pk.StatNature; 
            ((PK9)pkm).HT_Language= pk.HT_Language;
            ((PK9)pkm).Gender = pk.Gender;
            ((PK9)pkm).FixMemories();
            ((PK9)pkm).RefreshChecksum();

        }

        public void NotifySaveLoaded()
        {
            Console.WriteLine($"{Name} was notified that a Save File was just loaded.");
            TrainerInfo = new SimpleTrainerInfo() { OT = SaveFileEditor.SAV.OT, SID = SaveFileEditor.SAV.SID, TID = SaveFileEditor.SAV.TID, Context = SaveFileEditor.SAV.Context, Language = SaveFileEditor.SAV.Language };
        }

        public bool TryLoadFile(string filePath)
        {
            Console.WriteLine($"{Name} was provided with the file path, but chose to do nothing with it.");
            return false; // no action taken
        }
        private IEnumerable<IEncounterInfo> SearchDatabase()
        {


            var sav = SaveFileEditor.SAV;
            var pk = sav.BlankPKM;
            
           
            var versions = new[] { PKHeX.Core.GameVersion.SL, PKHeX.Core.GameVersion.VL };
            var specie =  new[] {ushort.Parse(species.Text) };
            ushort[] moves = new ushort[0];

            var results = GetAllSpeciesFormEncounters(specie, sav.Personal, versions, moves, pk);
           
            // return filtered results
            var comparer = new ReferenceComparer<IEncounterInfo>();
            results = results.Distinct(comparer); // only distinct objects

            return results;
        }
        private static IEnumerable<IEncounterInfo> GetAllSpeciesFormEncounters(IEnumerable<ushort> species, IPersonalTable pt, IReadOnlyList<GameVersion> versions, ushort[] moves, PKM pk)
        {
            foreach (var s in species)
            {
                

                var pi = pt.GetFormEntry(s, 0);
                var fc = pi.FormCount;
                
                for (byte f = 0; f < fc; f++)
                {
                    if (FormInfo.IsBattleOnlyForm(s, f, pk.Format))
                        continue;
                    var encs = GetEncounters(s, f, moves, pk, versions);
                    foreach (var enc in encs)
                        yield return enc;
                }
            }
        }
        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            public bool Equals(T? x, T? y)
            {
                if (x == null)
                    return false;
                if (y == null)
                    return false;
                return RuntimeHelpers.GetHashCode(x).Equals(RuntimeHelpers.GetHashCode(y));
            }

            public int GetHashCode(T obj)
            {
                if (obj == null) throw new ArgumentNullException(nameof(obj));
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
        private static IEnumerable<IEncounterInfo> GetEncounters(ushort species, byte form, ushort[] moves, PKM pk, IReadOnlyList<GameVersion> vers)
        {
            pk.Species = species;
            pk.Form = form;
            pk.SetGender(pk.GetSaneGender());
            return EncounterMovesetGenerator.GenerateEncounters(pk, moves, vers);
        }

        public void generateForm()
        {
            SaveFile sav = SaveFileEditor.SAV; // current savefile

            List<Control> formControls = new List<Control>();
            Button createButton = new Button();

            // Set up form
            formui.Size = new System.Drawing.Size(390, 500);
            formui.Name = "Brueforcer UI";

            // Set up Type Selection
            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(8, 10),
                AutoSize = true,
                Text = "species\nnational dex number",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });
            species.Text = "132";
            species.Location = new System.Drawing.Point(10, 50);
            formControls.Add(species);

            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(10, 85),
                AutoSize = true,
                Text = "Nature in pkhex enums \nset to 25 for random",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });
            Nature.Text = "25";
            Nature.Location = new System.Drawing.Point(10, 125);
            formControls.Add(Nature);
            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(170, 10),
                AutoSize = true,
                Text = "HP",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });

            HP.Text = "31";
            HP.Location = new System.Drawing.Point(170, 25);
            HPc.Location = new System.Drawing.Point(300, 25);
            formControls.Add(HPc);
            formControls.Add(HP);

          
            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(170, 55),
                AutoSize = true,
                Text = "ATK",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });

            ATK.Text = "31";
            ATK.Location = new System.Drawing.Point(170, 70);
            ATKc.Location = new System.Drawing.Point(300, 70);
            formControls.Add(ATKc);
            formControls.Add(ATK);

       
            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(170, 95),
                AutoSize = true,
                Text = "DEF",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });

            DEF.Text = "31";
            DEF.Location = new System.Drawing.Point(170, 115);
            DEFc.Location = new System.Drawing.Point(300, 115);
            formControls.Add(DEF);
            formControls.Add(DEFc);

            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(170, 145),
                AutoSize = true,
                Text = "SPA",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });


            SPA.Text = "31";

            SPA.Location = new System.Drawing.Point(170, 160);
            SPAc.Location = new System.Drawing.Point(300, 160);
            formControls.Add(SPA); 
            formControls.Add(SPAc);

            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(170, 190),
                AutoSize = true,
                Text = "SPD",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });

            SPD.Text = "31";
            SPD.Location = new System.Drawing.Point(170, 205);
            SPDc.Location = new System.Drawing.Point(300, 205);
            formControls.Add(SPD); 
            formControls.Add(SPDc);
            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(170, 235),
                AutoSize = true,
                Text = "SPE",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold)
            });

            SPE.Text = "31";
            SPE.Location = new System.Drawing.Point(170, 250); 
            SPEc.Location = new System.Drawing.Point(300, 250);
            formControls.Add(SPE);
            formControls.Add(SPEc);

            createButton.Text = "Start Bruteforcing";
            createButton.Size = new System.Drawing.Size(185, 20);
            createButton.Location = new System.Drawing.Point(80, 300);
            createButton.Click += new EventHandler(startbuttoonclick);

            formControls.Add(createButton);

            formControls.Add(new Label
            {
                Location = new System.Drawing.Point(80, 330),
                AutoSize = true,
                Text = "Warning if more than 2 stats is selected \nthe work to bruteforce increase exponentially\nI suggest only selecting stats to be 0",
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Regular)
            });

            // Add everything to the form, and show it
            formui.Controls.AddRange(formControls.ToArray());
            formui.ShowDialog();
        }
        public void startbuttoonclick(Object sender, EventArgs events)
        {
            ModifySaveFile();
        }
        


    }

}
