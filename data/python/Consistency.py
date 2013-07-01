# minimal dependency forest for single destination, by Roger

import csv
import copy

inputfile  = "Files/rules.txt"
outputfile = "Files/forest.txt"

unknown = 1
seen = 2
visited = 3
old = 4
limbo = 5
new = 6
root = -1
nobody = -2

#read rules from file:
reader = csv.reader(open(inputfile,"r"),delimiter=",")
rules = []
n = int(reader.next()[0])
dest = int(reader.next()[0])
for line in reader:
	active = 0
	if int(line[2]) == 0: active = 1
	rules.append([int(line[0]),int(line[1]),int(line[2]),active])

node = []
for i in range(0,n):
	node.append([i,[]])

#rule = [node, next, old 0 or new 1, currently active]
for rule in rules:
	node[rule[0]][1].append(rule)

def newrule(u):
	mynode = node[u]
	myrules = node[u][1]
	for i in range(0,len(myrules)):
		rule = myrules[i]
		if rule[2] == 1:
			return rule

def children(w):
	myrules = node[w][1]
	for i in range(0,len(myrules)):
		rule = myrules[i]
		if rule[3] == 1:
			if dfs(rule[1]) == True: return True
	return False

def dfs(w):
	#print("dfs of",w,loopstate)
	if loopstate[w] == unknown:
		loopstate[w] = seen
		if children(w) == True: return True
	elif loopstate[w] == seen:
		#print("loop found")
		return True
	elif loopstate[w] == visited:
		return False

def testloop(rule):
	global loopstate
	loopstate = []
	for u in range(0,n):
		loopstate.append(unknown)
	loopstate[rule[0]] = seen
	return dfs(rule[1])

# initialize forest for destination t
def init():
	for u in range(0,n):
		print("init for node",u)
		state.append(old)
		if u==dest: 
			parent.append(root) # destination is a root 
			state[u] = new
		else:
			mynewrule = newrule(u)
			if mynewrule == None:
				parent.append(root) # no loop found, is a root!  
				state[u] = new
			else:
				mynewrule[3] = 1
				#print("testloop",mynewrule)
				res = testloop(mynewrule)
				#print("result",res)
				if not res:
					parent.append(root) # no loop found, is a root!  
					state[u] = limbo
				else:
					mynewrule[3] = 0 # loop found, must find real parent!
					#print("here",parent)
					parent.append(nobody)

#TODO: now find parents!
def findchildren(s): # s is node which just removed old pointer, t is destination
	for u in range(0,n):
		if state[u] == old:
			mynewrule = newrule(u)
			mynewrule[3] = 1
			#print(mynewrule)
			res = testloop(mynewrule)
			if not res:
				parent[u] = s # no loop found, is a child of s!
				state[u] = limbo
				print("found parent of node",u)
			else:
				mynewrule[3] = 0 # loop found, must find real parent later!

def finddirectnew():
	for u in range(0,n):
		if state[u] == old:
			#first memorize all the old rules currently active:
			myrules = node[u][1]
			temprules = copy.deepcopy(myrules)
			for k in range(0,len(myrules)):
				if myrules[k][3] == 1: myrules[k][3] = 0
			mynewrule = newrule(u)
			mynewrule[3] = 1
			#print(mynewrule)
			res = testloop(mynewrule)
			#print(res)
			if not res:
				parent[u] = mynewrule[1] # no loop found, is a child node at end of new pointer!  
				state[u] = new
				print("direct switch of node",u)
			else:
				node[u][1] = copy.deepcopy(temprules) # loop found, return to previous!

def nodesinstate(st):
	sum = 0
	for u in range(0,n):
		if state[u] == st:
			sum += 1
	return sum

def finddependencyforest():
	init()
	while nodesinstate(new) < n:
		if nodesinstate(limbo) > 0:
			for u in range(0,n):
				if state[u] == limbo:
					myrules = node[u][1]
					for k in range(0,len(myrules)):
						if myrules[k][3] == 1: myrules[k][3] = 0
					mynewrule = newrule(u)
					mynewrule[3] = 1
					state[u] = new
					#print("find children of:",mynewrule)
					findchildren(u)
					#print("state",state,"parent",parent)
		elif nodesinstate(old) > 0:
			print("find direct...",state)
			finddirectnew()
	finddepth()

def finddepth():
	for u in range(0,n):
		x = u
		depth.append(0)
		while parent[x] != -1:
			depth[u] += 1
			x = parent[x]

		


state = []
loopstate = []
parent = []
depth = []
finddependencyforest()

#state = []
#loopstate = []
#parent = []
#init()

f = open(outputfile,'wb')
mywriter = csv.writer(f, delimiter=',')
for k in range(0,n):
	mywriter.writerow([k,parent[k],depth[k]])

f.close()
