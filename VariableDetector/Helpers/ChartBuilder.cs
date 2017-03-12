using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableDetector.Models;
using GAF;
using GAF.Operators;

namespace VariableDetector.Helpers
{
    public class ChartBuilder
    {
        class StarComparer : IEqualityComparer<Star>
        {
            public bool Equals(Star x, Star y)
            {
                return x.Name.Equals(y.Name);
            }

            public int GetHashCode(Star obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private StarField _field;
        private List<Star> _eligiblestars;
        private List<Star> _elitestars;
        private List<Star> _teststars;

        private Random _rnd = new Random();
 
        public ChartBuilder(StarField field, List<Star> eligiblestars, List<Star> elitestars)
        {
            _field = field;
            _eligiblestars = eligiblestars;
            _elitestars = elitestars;
            _teststars = eligiblestars;
            //_teststars = field.Stars
            //    .Where(x => x.MinSNR > GlobalVariables.MinSNR && x.Flags == 0 && x.ValidCatalogMag == true && x.OID == -1)
            //    .ToList();
        }

        public List<Star> Solve()
        {
            const double crossoverProbability = 0.75;
            const double mutationProbability = 0.5;
            const int elitismPercentage = 5;

            var population = new Population();

            //create the chromosomes
            for (var p = 0; p < 100; p++)
            {

                var chromosome = new Chromosome();
                for (var g = 0; g < GlobalVariables.Comparables - _elitestars.Count; g++)
                {
                    chromosome.Genes.Add(new Gene(_rnd.NextDouble()));
                }
                population.Solutions.Add(chromosome);
            }

            //create the genetic operators 
            var elite = new Elite(elitismPercentage);

            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            //create the GA itself 
            var ga = new GeneticAlgorithm(population, EvaluateQuality);

            //subscribe to the GAs Generation Complete event 
            ga.OnGenerationComplete += ga_OnGenerationComplete;

            //add the operators to the ga process pipeline 
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            //run the GA 
            ga.Run(TerminateAlgorithm);

            List<Star> picks = new List<Star>();
            picks.AddRange(_elitestars);
            
            foreach (Gene gene in population.GetTop(1)[0].Genes)
            {
                picks.Add(_eligiblestars[(int)(System.Math.Abs(gene.RealValue) * _eligiblestars.Count)]);
            }
            
            FastReduction();

            var mag = _teststars
                .Select(x => (x.VMag - x.CatalogEntry.Vmag));
            Console.WriteLine();
            Console.WriteLine("Best fit mean     : " + mag.Average());
            Console.WriteLine("Best fit deviation: " + mag.StdDev());

            picks = picks.Distinct(new StarComparer()).ToList();
            return picks;
        }

        public bool TerminateAlgorithm(Population population, int currentGeneration, long currentEvaluation)
        {
            Console.Write("\rIterations complete: " + currentGeneration);
            return currentGeneration > 100;
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            //get the best solution 
            var chromosome = e.Population.GetTop(1)[0];
        }

        public double EvaluateQuality(Chromosome chromosome)
        {
            if (chromosome != null)
            {
                List<Star> picks = new List<Star>();
                picks.AddRange(_elitestars);

                foreach (Gene gene in chromosome.Genes)
                {
                    picks.Add(_eligiblestars[(int)(System.Math.Abs(gene.RealValue) * _eligiblestars.Count)]);
                }                

                _field.ChartComparables.Clear();
                _field.ChartComparables.AddRange(picks);
                _field.ChartComparables = _field.ChartComparables.Distinct().ToList();

                if (_field.ChartComparables.Count() < 3)
                    return 0.0;

                FastReduction();

                var mag = _teststars
                    .Select(x => (x.VMag - x.CatalogEntry.Vmag)/10.0m);

                var var = _teststars
                    .Select(x => (x.e_VMag));

                var color = _teststars
                    .Select(x => (x.CalculatedColorIndex - x.ColorIndex)/10.0m);

                return 1 - (
                    System.Math.Abs((double)var.Average()) *
                    System.Math.Abs((double)color.StdDev())*
                    System.Math.Abs((double)color.Average())*
                    System.Math.Abs((double)mag.StdDev()) *
                    System.Math.Abs((double)mag.Average()));
            }
            else
            {
                //chromosome is null
                throw new ArgumentNullException("chromosome",
                    "The specified Chromosome is null.");
            }
        }

        public void FastReduction()
        {
            List<Star> comparables = _field.GetComparables(_elitestars.First());
            foreach (Star star in _teststars)
            {
                _field.CalcBVEstimate(star, comparables);

                // Estimate target vmag and error (error might include variable signal).
                _field.CalcVMagEstimate(star, comparables);
            }
        }
    }
}
