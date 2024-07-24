--get to GC desk (Your GC desk)
--usual scripts and stuff needed.

--0 = serpents, 1 = maelstrom, 2 = flames
local franchise_owners = {
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0},
{"Firstname Lastname@Server", 0}
}

loadfiyel = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\_functions.lua"
functionsToLoad = loadfile(loadfiyel)
functionsToLoad()
DidWeLoadcorrectly()

for i=1,#franchise_owners do
	if GetCharacterName(true) ~= franchise_owners[i][1] then
		yield("/ays relog " ..franchise_owners[i][1])
		yield("/wait 2")
		CharacterSafeWait()
	end	
	
	if franchise_owners[i][2] == 0 then
		yield("/li gc serpent")
	end	
	if franchise_owners[i][2] == 1 then
		yield("/li gc maelstrom")
		yield("/wait 15")
		yield("/li gc maelstrom")
	end	
	if franchise_owners[i][2] == 2 then
		yield("/li gc flames")
	end
	yield("/wait 15")
	visland_stop_moving()
end