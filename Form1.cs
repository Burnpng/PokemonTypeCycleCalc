using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonTypeCycleGUI
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<string, TypeEffectiveness> typeChart = new Dictionary<string, TypeEffectiveness>
        {
            { "Normal", new TypeEffectiveness(new HashSet<string> { "Rock", "Steel" }, new HashSet<string> { "Ghost" }) },
            { "Fire", new TypeEffectiveness(new HashSet<string> { "Grass", "Ice", "Bug", "Steel" }, new HashSet<string> { "Water", "Rock", "Fire", "Dragon" }) },
            { "Water", new TypeEffectiveness(new HashSet<string> { "Fire", "Ground", "Rock" }, new HashSet<string> { "Grass", "Water", "Dragon" }) },
            { "Grass", new TypeEffectiveness(new HashSet<string> { "Water", "Ground", "Rock" }, new HashSet<string> { "Fire", "Grass", "Poison", "Flying", "Bug", "Dragon", "Steel" }) },
            { "Electric", new TypeEffectiveness(new HashSet<string> { "Water", "Flying" }, new HashSet<string> { "Grass", "Electric", "Dragon" }, new HashSet<string> { "Ground" }) },
            { "Ice", new TypeEffectiveness(new HashSet<string> { "Grass", "Ground", "Flying", "Dragon" }, new HashSet<string> { "Fire", "Water", "Ice", "Steel" }) },
            { "Fighting", new TypeEffectiveness(new HashSet<string> { "Normal", "Ice", "Rock", "Dark", "Steel" }, new HashSet<string> { "Poison", "Flying", "Psychic", "Bug", "Fairy" }, new HashSet<string> { "Ghost" }) },
            { "Poison", new TypeEffectiveness(new HashSet<string> { "Grass", "Fairy" }, new HashSet<string> { "Poison", "Ground", "Rock", "Ghost" }, new HashSet<string> { "Steel" }) },
            { "Ground", new TypeEffectiveness(new HashSet<string> { "Fire", "Electric", "Poison", "Rock", "Steel" }, new HashSet<string> { "Grass", "Bug" }, new HashSet<string> { "Flying" }) },
            { "Flying", new TypeEffectiveness(new HashSet<string> { "Grass", "Fighting", "Bug" }, new HashSet<string> { "Electric", "Rock", "Steel" }) },
            { "Psychic", new TypeEffectiveness(new HashSet<string> { "Fighting", "Poison" }, new HashSet<string> { "Psychic", "Steel" }, new HashSet<string> { "Dark" }) },
            { "Bug", new TypeEffectiveness(new HashSet<string> { "Grass", "Psychic", "Dark" }, new HashSet<string> { "Fire", "Fighting", "Poison", "Flying", "Ghost", "Steel", "Fairy" }) },
            { "Rock", new TypeEffectiveness(new HashSet<string> { "Fire", "Ice", "Flying", "Bug" }, new HashSet<string> { "Fighting", "Ground", "Steel" }) },
            { "Ghost", new TypeEffectiveness(new HashSet<string> { "Psychic", "Ghost" }, new HashSet<string> { "Dark" }, new HashSet<string> { "Normal" }) },
            { "Dragon", new TypeEffectiveness(new HashSet<string> { "Dragon" }, new HashSet<string> { "Steel" }, new HashSet<string> { "Fairy" }) },
            { "Dark", new TypeEffectiveness(new HashSet<string> { "Psychic", "Ghost" }, new HashSet<string> { "Fighting", "Dark", "Fairy" }) },
            { "Steel", new TypeEffectiveness(new HashSet<string> { "Ice", "Rock", "Fairy" }, new HashSet<string> { "Fire", "Water", "Electric", "Steel" }) },
            { "Fairy", new TypeEffectiveness(new HashSet<string> { "Fighting", "Dragon", "Dark" }, new HashSet<string> { "Fire", "Poison", "Steel" }) }
        };

        private readonly Dictionary<string, string> typeIcons = new Dictionary<string, string>
        {
            { "Normal", "TypeIcons/Normal.png" },
            { "Fire", "TypeIcons/Fire.png" },
            { "Water", "TypeIcons/Water.png" },
            { "Grass", "TypeIcons/Grass.png" },
            { "Electric", "TypeIcons/Electric.png" },
            { "Ice", "TypeIcons/Ice.png" },
            { "Fighting", "TypeIcons/Fighting.png" },
            { "Poison", "TypeIcons/Poison.png" },
            { "Ground", "TypeIcons/Ground.png" },
            { "Flying", "TypeIcons/Flying.png" },
            { "Psychic", "TypeIcons/Psychic.png" },
            { "Bug", "TypeIcons/Bug.png" },
            { "Rock", "TypeIcons/Rock.png" },
            { "Ghost", "TypeIcons/Ghost.png" },
            { "Dragon", "TypeIcons/Dragon.png" },
            { "Dark", "TypeIcons/Dark.png" },
            { "Steel", "TypeIcons/Steel.png" },
            { "Fairy", "TypeIcons/Fairy.png" }
        };

        public Form1()
        {
            InitializeComponent();
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            for (var i = 3; i <= 6; i++)
            {
                CycleLengthComboBox.Items.Add(i);
            }
            CycleLengthComboBox.SelectedIndex = 0;
            CycleLengthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private async void GenerateButton_Click(object sender, EventArgs e)
        {
            CycleIconsPanel.Controls.Clear();
            var cycleLength = (int)CycleLengthComboBox.SelectedItem;
            GenerateButton.Enabled = false;

            var perfectCyclesTask = Task.Run(() => FindPerfectCycles(cycleLength));
            var immunityCyclesTask = Task.Run(() => FindImmunityCycles(cycleLength));

            var results = await Task.WhenAll(perfectCyclesTask, immunityCyclesTask);

            DisplayResults("Perfect Symmetrical Cycles:", results[0]);
            DisplayResults("Symmetrical Cycles with Immunities:", results[1]);

            GenerateButton.Enabled = true;
        }

        private void DisplayResults(string title, List<List<string>> cycles)
        {
            AddTitleLabel(title);

            if (cycles.Count > 0)
            {
                foreach (var cycle in cycles)
                {
                    var cyclePanel = CreateCyclePanel(cycle);
                    CycleIconsPanel.Controls.Add(cyclePanel);
                }
            }
            else
            {
                AddNoneLabel();
            }

            AddSeparator();
        }

        private void AddTitleLabel(string title)
        {
            var titleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            CycleIconsPanel.Controls.Add(titleLabel);
        }

        private FlowLayoutPanel CreateCyclePanel(List<string> cycle)
        {
            var cyclePanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            foreach (var type in cycle)
            {
                var icon = new PictureBox
                {
                    Image = Image.FromFile(typeIcons[type]),
                    SizeMode = PictureBoxSizeMode.AutoSize
                };
                cyclePanel.Controls.Add(icon);

                if (type != cycle.Last())
                {
                    var arrowLabel = new Label
                    {
                        Text = "→",
                        AutoSize = true,
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        Padding = new Padding(5, 0, 5, 0)
                    };
                    cyclePanel.Controls.Add(arrowLabel);
                }
            }

            return cyclePanel;
        }

        private void AddNoneLabel()
        {
            var noneLabel = new Label
            {
                Text = "None",
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Regular)
            };
            CycleIconsPanel.Controls.Add(noneLabel);
        }

        private void AddSeparator()
        {
            CycleIconsPanel.Controls.Add(new Label { Text = "", AutoSize = true });
        }

        private List<List<string>> FindPerfectCycles(int cycleLength)
        {
            var types = typeChart.Keys.ToList();
            var combinations = GenerateCombinations(types, cycleLength);
            return combinations.Where(IsPerfectCycle).Select(NormalizeCycle).Distinct(new CycleComparer()).ToList();
        }

        private List<List<string>> FindImmunityCycles(int cycleLength)
        {
            var types = typeChart.Keys.ToList();
            var combinations = GenerateCombinations(types, cycleLength);
            return combinations.Where(IsImmunityCycle).Select(NormalizeCycle).Distinct(new CycleComparer()).ToList();
        }

        private bool IsPerfectCycle(List<string> types)
        {
            for (var i = 0; i < types.Count; i++)
            {
                var currentType = types[i];
                var nextType = types[(i + 1) % types.Count];

                if (!typeChart[currentType].SuperEffective.Contains(nextType) ||
                    !typeChart[nextType].NotVeryEffective.Contains(currentType) ||
                    typeChart[currentType].Immune.Contains(nextType))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsImmunityCycle(List<string> types)
        {
            var hasImmunity = false;

            for (var i = 0; i < types.Count; i++)
            {
                var currentType = types[i];
                var nextType = types[(i + 1) % types.Count];

                if (!typeChart[currentType].SuperEffective.Contains(nextType) ||
                    (!typeChart[nextType].NotVeryEffective.Contains(currentType) && !typeChart[nextType].Immune.Contains(currentType)))
                {
                    return false;
                }

                if (typeChart[nextType].Immune.Contains(currentType))
                {
                    hasImmunity = true;
                }
            }

            return hasImmunity;
        }

        private List<List<string>> GenerateCombinations(List<string> types, int length)
        {
            return types.SelectMany(t => GenerateCombinationsRecursive(types, length, new List<string> { t })).ToList();
        }

        private IEnumerable<List<string>> GenerateCombinationsRecursive(List<string> types, int length, List<string> current)
        {
            if (current.Count == length)
            {
                yield return new List<string>(current);
                yield break;
            }

            foreach (var type in types)
            {
                if (!current.Contains(type))
                {
                    current.Add(type);
                    foreach (var combination in GenerateCombinationsRecursive(types, length, current))
                    {
                        yield return combination;
                    }
                    current.RemoveAt(current.Count - 1);
                }
            }
        }

        private List<string> NormalizeCycle(List<string> cycle)
        {
            var minIndex = cycle.Select((type, index) => new { type, index })
                                .OrderBy(x => x.type)
                                .First().index;

            return cycle.Skip(minIndex).Concat(cycle.Take(minIndex)).ToList();
        }

        private class CycleComparer : IEqualityComparer<List<string>>
        {
            public bool Equals(List<string> x, List<string> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<string> obj)
            {
                return obj.Aggregate(0, (hash, type) => hash ^ type.GetHashCode());
            }
        }

        private class TypeEffectiveness
        {
            public HashSet<string> SuperEffective { get; }
            public HashSet<string> NotVeryEffective { get; }
            public HashSet<string> Immune { get; }

            public TypeEffectiveness(HashSet<string> superEffective, HashSet<string> notVeryEffective, HashSet<string> immune = null)
            {
                SuperEffective = superEffective;
                NotVeryEffective = notVeryEffective;
                Immune = immune ?? new HashSet<string>();
            }
        }
    }
}
