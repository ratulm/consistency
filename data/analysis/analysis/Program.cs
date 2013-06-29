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

        //dest->source
        Dictionary<string, Dictionary<string, int>> links = new Dictionary<string,Dictionary<string,int>>();

        Random random = new Random();

        public Program(string[] args)
        {
            fileName = args[0];

            ParseFile(fileName);
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

                AddLink(tokens[1], tokens[0], int.Parse(tokens[2]));
            }

            //write the total number of nodes
            Console.WriteLine("{0}", nodes.Count);

            //pick a random destination and write it to file
            string destination = GetNodeFromIndex(random.Next(nodes.Count));
            Console.WriteLine("{0}", destination);

            //compute and print paths
            var originalPaths = ComputeShortestPaths(destination, links);

            PrintPaths(originalPaths, "0");

            //pick a link to fail
            string linkDest = GetNodeFromIndex(random.Next(nodes.Count));

            // ... don't pick a link with one neighbor
            while (links[linkDest].Count < 2)
            {
                linkDest = GetNodeFromIndex(random.Next(nodes.Count));
            }

            //.... pick a neighbor
            string linkSrc = GetNodeFromIndex(random.Next(links[linkDest].Count));

            //remove the links
            links[linkDest].Remove(linkSrc);

            //compute and print paths again
            var newPaths = ComputeShortestPaths(destination, links);

            PrintPaths(newPaths, "1");

        }

        private void PrintPaths(Dictionary<string, Tuple<string, int>> paths, string type)
        {
            foreach (var source in paths.Keys)
            {
                Console.WriteLine("{0} {1} {2}", source, paths[source].Item1, type);
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

        private void AddLink(string node1, string node2, int cost)
        {
            if (!links.ContainsKey(node1))
                links.Add(node1, new Dictionary<string, int>());

            links[node1].Add(node2, cost);
        }

        //source->(nexthop, cost)
        private Dictionary<string, Tuple<string, int>> ComputeShortestPaths(string destination, Dictionary<string, Dictionary<string, int>> links)
        {
            var paths = new Dictionary<string, Tuple<string, int>>();

            //first initialize the paths
            foreach (var source in nodes.Keys)
            {
                if (source.Equals(destination))
                   paths.Add(source, new Tuple<string, int>(source, 0));
                else 
                   paths.Add(source, new Tuple<string,int>(null, int.MaxValue));
            }

            bool somethingChanged = true;

            while (somethingChanged)
            {
                foreach (var source in links.Keys)
                {
                    int leastCost = paths[source].Item2;

                    foreach (var neighbor in links[source].Keys)
                    {
                        int newCost = paths[neighbor].Item2 + links[source][neighbor];

                        if (newCost < leastCost)
                        {
                            somethingChanged = true;

                            paths[source] = new Tuple<string, int>(neighbor, newCost);

                            leastCost = newCost;
                        }
                    }
                }

            }

            return paths;

        }
    }
}
