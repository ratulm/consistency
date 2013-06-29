using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace analysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new Program(args);
        }

        string fileName;

        Dictionary<string, int> nodes = new Dictionary<string,int>();

        //dest->[source, cost]
        Dictionary<string, List<Tuple<string, double>>> revLinks = new Dictionary<string, List<Tuple<string,double>>>();

        Random random = new Random();

        public Program(string[] args)
        {
            fileName = args[0];

            ParseFile(fileName);

            //write the total number of nodes
            Console.WriteLine("{0}", nodes.Count);

            //pick a random destination and write it to file
            string destination = GetNodeFromIndex(random.Next(nodes.Count));
            Console.Error.WriteLine("Picked destination: {0}", destination);
            Console.WriteLine("{0}", nodes[destination]);

            //compute and print paths
            var originalPaths = ComputeShortestPaths(destination);

            PrintPaths(originalPaths, "0");

            //pick a link to fail
            string linkDest = GetNodeFromIndex(random.Next(nodes.Count));

            // ... don't pick a link with one neighbor
            while (revLinks[linkDest].Count < 2)
            {
                linkDest = GetNodeFromIndex(random.Next(nodes.Count));
            }

            //.... pick a neighbor
            var linkIndex = random.Next(revLinks[linkDest].Count);
            var linkSrcTuple = revLinks[linkDest][linkIndex];

            Console.Error.WriteLine("Removed link: {0}->{1}", linkSrcTuple.Item1, linkDest);

            //remove the links
            revLinks[linkDest].RemoveAt(linkIndex);

            //compute and print paths again
            var newPaths = ComputeShortestPaths(destination);

            PrintPaths(newPaths, "1");

            //Console.ReadLine();
        }

        void ParseFile(string fileName)
        {
            var file = new StreamReader(File.OpenRead(fileName));

            while (!file.EndOfStream)
            {
                var line = file.ReadLine();

                //node1 node2 cost
                string[] tokens = line.Split(' ');

                RegisterNode(tokens[0]);
                RegisterNode(tokens[1]);

                AddReverseLink(tokens[1], tokens[0], double.Parse(tokens[2]));
            }
        }

        private void PrintPaths(Dictionary<string, Tuple<string, double>> paths, string type)
        {
            foreach (var source in paths.Keys)
            {
                if (paths[source].Item1 == null)
                {
                    Console.Error.WriteLine("{0} does not have a path", source);
                    continue;
                }

                if (paths[source].Item1.Equals(source))
                {
                    Console.Error.WriteLine("skipping {0} as it appears to be the destination", source);
                    continue;
                }

                Console.WriteLine("{0},{1},{2} {3}", nodes[source], nodes[paths[source].Item1], type, paths[source].Item2);
                //Console.WriteLine("{0},{1},{2}", nodes[source], nodes[paths[source].Item1], type);
            }
        }


        private string GetNodeFromIndex(int nodeIndex)
        {
            foreach (var nodeName in nodes.Keys)
            {
                if (nodes[nodeName] == nodeIndex)
                    return nodeName;
            }

            throw new Exception("Index not found!");
        }

        private void RegisterNode(string nodeName)
        {
            if (!nodes.ContainsKey(nodeName))
                nodes.Add(nodeName, nodes.Count);                
        }

        private void AddReverseLink(string dest, string src, double cost)
        {
            if (!revLinks.ContainsKey(dest))
                revLinks.Add(dest, new List<Tuple<string, double>>());

            revLinks[dest].Add(new Tuple<string, double>(src, cost));
        }

        //source->(nexthop, cost)
        private Dictionary<string, Tuple<string, double>> ComputeShortestPaths(string destination)
        {
            var paths = new Dictionary<string, Tuple<string, double>>();

            //first initialize the paths
            foreach (var source in nodes.Keys)
            {
                if (source.Equals(destination))
                   paths.Add(source, new Tuple<string, double>(source, 0));
                else 
                   paths.Add(source, new Tuple<string,double>(null, double.MaxValue));
            }

            bool somethingChanged = true;

            while (somethingChanged)
            {
                somethingChanged = false;

                //pick a node and "advertise" its path to all its upstream neighbors
                foreach (var node in revLinks.Keys)
                {
                    foreach (var upNbrTuple in revLinks[node])
                    {
                        double currCost = paths[upNbrTuple.Item1].Item2;

                        double newCost = paths[node].Item2 + upNbrTuple.Item2;

                        if (newCost < currCost)
                        {
                            somethingChanged = true;

                            paths[upNbrTuple.Item1] = new Tuple<string, double>(node, newCost);
                        }
                    }
                }
            }

            return paths;

        }
    }
}
