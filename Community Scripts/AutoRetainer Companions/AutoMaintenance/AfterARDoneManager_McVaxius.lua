--[[
After AR Done Manager
What does it do?

configure AR to run a script after AR is done and it have it run THIS script.
This script will, aftre AR is done, do various things based on a set of rules you will configure in a separate file (AADMconfig_McVaxius.lua)

It could be ocean fishing, triple triad, inventory cleaning, going for a jog around the housing ward, delivering something to specific person, crafting. or whatever!

Requirements : SND
and maybe more - let's see where we go with it

{"Firstname Lastname@Server", 0},

order of configs
from first ?, to last starting cardinality of 1

1 = full char name with @server, it is case sensitive and spelling sensitive please have a spreadsheet for this stuff folks
2 = return location for any operations that require teleporting and returning. standard locations from robust turnin apply. 0 = fc, 1 = near fc but bell, etc ill document later --*
3 = chance to clean inventory (check _functions.lua for details) - don't ask about this in the punish disc i wont respond and at best ill just block you. you can ask in liza disc if you want
4 = number of minutes of TT to run from 0 to whatever
5 = TT location to run, 1=arena roe, 2=manservant
6 = magitek repair kits to print (it will require you to have a npc in your house with g6dm vendor available. preferrably easily pathable from entrance)
7 = fuel restock safety stock - amount of fuel where we buy some more fuel to refill the coffers. it will check inventory slots free etc to make sure you can do it. maybe output to an "empire log" file if there is an issue
8 = fuel restock refuel amount - how much actual fuel to buy up to 
9 = cuff a curr bonkings expressed in a amount of MGP to acquire each time from 0 to whatever
10 = FC buff 1 to refresh if its down
11 = FC buff 2 to refresh if its down
12 = fisher level. if its >0 and <100 (for now unless level cap changes) it will assume you want this char to go ocean fishing.  it will iterate through the list at certain time of day and do ocean fishing on the lowest level char, and it will also update the table and output it
--]]


loadfiyel = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\_functions.lua"
functionsToLoad = loadfile(loadfiyel)
AADM_processors = {
{"Firstname Lastname@Server", 0, 10, 2, 60, 20, 666, 6666, 50, "Helping Hand II", "Make it Rain II", 100},   --your main char for example, 1 hour of manservant every refresh, 15 magitek repair kits every refresh, restock fuel to 6666 at 666 fuel remaining.
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 32},    --this char will be picked next for ocean fishing
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92},
{"Firstname Lastname@Server", 0, 10, 0,  0,  0,   0,    0,  0,         "nothing",         "nothing", 92}
}
loadfiyel2 = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\AADMconfig_McVaxius.lua"
functionsToLoad = loadfile(loadfiyel2)

function getRandomNumber(min, max)
  return math.random(min,max)
end

ungabungabunga()  --get out of anything safely.

hoo_arr_weeeeee = 1 -- who are we

for i=1,#AADM_processors do
	if GetCharacterName(true) == AADM_processors[i][1] then
		hoo_arr_weeeeee = i
	end
end

--begin to do stuff
------------------------------------

if AADM_processors[hoo_arr_weeeeee][2] > 0 then
	if getRandomNumber(0,99) < AADM_processors[hoo_arr_weeeeee][2] then
		clean_inventory()
		ungabungabunga()
		--*gc cleaning
	end
end


------------------------------------
--stop beginning to do stuff
	ungabungabunga()
end

