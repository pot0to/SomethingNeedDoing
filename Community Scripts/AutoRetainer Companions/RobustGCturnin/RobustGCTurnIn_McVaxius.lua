--[[
  Description: Updated Deliver and clean up script for using deliveroo and visland.
  Author: McVaxius
  Link: https://discord.com/channels/1162031769403543643/1162799234874093661/1190858719546835065
]]

--enter in names of characters that will be responsible for triggering FC Buffs
local chars_FCBUFF = {
 "First Last@Server",
 "First Last@Server",
 "First Last@Server",
 "First Last@Server",
 "First Last@Server"
}

--characters with servername, fc house or bell (0, 1) or 2 for gridania inn
local chars_fn = {
 {"First Last@Server", 0},
 {"First Last@Server", 0},
 {"First Last@Server", 0},
 {"First Last@Server", 0},
 {"First Last@Server", 0},
 {"First Last@Server", 0},
 {"First Last@Server", 0},
 {"First Last@Server", 0}
}

--starting the counter at 1
local rcuck_count = 1
--do we bother with fc buffs? 0 = no 1 = yes
local process_fc_buffs = 1
--do we run each city?
local process_players = 1


yield("/ays multi d")

--setup yesalready for the inn if you need it. use the list option to pick appropriate entry.

--some ideas for next version
--deliveroo config suggestion: add some seals. and we can have a seal 0 or 1 option in settings
--add instructions for how to use this script
--separate config into a file
--check direction of where we spawned in gridania and uldah to adjust, and include new vislands
--change any vislands to use base64 var passed to visland
--use snd useitem
--https://discord.com/channels/1001823907193552978/1196163718216679514/1215227696607531078

--borrowed some code and ideas from the wonderful:  (make sure the _functions is in the snd folder)
--https://github.com/elijabesu/ffxiv-scripts/blob/main/snd/_functions.lua
loadfiyel = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\_functions.lua"
functionsToLoad = loadfile(loadfiyel)
functionsToLoad()
DidWeLoadcorrectly()

-- Specify the path to your text file
--[[
	--some vestigial junk i may remove if not needed once i update script properly
	tempchar = GetCharacterName()
	tempchar = tempchar:match("%s*(.-)%s*") --remove spaces at start and end only
	tempchar = tempchar:gsub("%s", "")  --remove all spaces
	tempchar = tempchar:gsub("'", "")   --remove all apostrophes
]]

function Final_GC_Cleaning()
	--turn around in case we aren't facing the correct way
	--this attempts to target serpent or flame personnel or even storm.  assuming you have a separate line in a hotkey for each type.
	--the purpose of this section is to get your char to face the npcs and orient the camera properly. otherwise the rest of the script might die

	--no targeting needed with deliveroo
	--yield("/send KEY_3")
	--yield("/wait 1")
	--yield("/lockon on")
	--yield("/automove on")
	--yield("/wait 1")

	visland_stop_moving() --just in case we want to auto equip rq before dumping gear
	--deliveroo i choose you
	yield("/deliveroo enable")
	yield("/wait 1")

	--loop until deliveroo done
	dellyroo = true
	dellyroo = DeliverooIsTurnInRunning()
	dellycount = 0
	while dellyroo do
		yield("/wait 5")
		dellyroo = DeliverooIsTurnInRunning()
		dellycount = dellycount + 1
		yield("/echo Processing Retainer Abuser "..rcuck_count.."/"..#chars_fn)
		if dellycount > 100 then
			--do some stuff like turning off deliveroo and targeting and untargeting an npc
			--i think we just need to target the quartermaster and open the dialog with him
			--this will solve getting stuck on deliveroo doing nothing
			dellycount = 0
		end
	end

	--added 5 second wait here because sometimes they get stuck.
	yield("/wait 5")
	
	--if we are tp to inn. we will go to gridania yo
	if chars_fn[rcuck_count][2] == 3 then
		yield("/tp New Gridania")
		ZoneTransition()
		yield("/wait 2")
		PathfindAndMoveTo(48.969123840332, -1.5844612121582, 57.311756134033, false)
		visland_stop_moving() --added so we don't accidentally end before we get to the inn person
		yield("/visland exectemponce H4sIAAAAAAAACu3WS4/TMBAA4L9S+RxGfo0fuaEFpBUqLLtIXUAcDPVSS01cEgeEqv53nDSlWxAHUI65eWxnNPlkjb0nr1zlSUm+NGHt6uAWO9ek4LaLFBehrklBVu7HLoY6taT8sCc3sQ0pxJqUe3JPSmnAKsu4LMg7Uj5hgEZKxXhB3pMSNQjGNKpDDmPtr5+Rkom8duvWocv5GNCCLOM3X/k6kTIHNy5tHkK9JmVqOl+Q6zr5xn1Oq5A2r/vv6eXcWH0us93E76eVXF/O/uC27aMUQ9GsIM+rmPwpVfLVOHw67BiDN51v0+Pxnf86BMv4aZy+S3F3Fev1qJFnXobt9ip245/cxi75y/JWLqRzXX30IjaXOfrJt6Hyy7yPHoo/vFGBEGj1kZuCNohIe/7srQwgo8hm7qm4FQObDzQ/cUtpKcU+ztwawQqrZ+3JtCVIjsIctTkwTRH1YG0E5GNOJc7ak2nz3D14Bj9yK1BaS4sDt0VApZWdtSdr3AwYNxbHVmKASmWVPnIzKkFwTs3fvMV8Uf6jt8TcrM3wEjl7SzNyCxDa6rmZTHa8uQWRH4LmzP3rYDPUcr4kp5PWoASXVv0uzYAzIebH339Kfzz8BLifXG8MDQAA")
		visland_stop_moving() --added so we don't accidentally end before we get to the inn person
		yield("/target Antoinaut")
		yield("/wait 0.5")
		yield("/interact")
	end
	
	--options 1 and 2 are fc estate entrance or fc state bell so thats only time we will tp to fc estate
	if chars_fn[rcuck_count][2] == 0 or if chars_fn[rcuck_count][2] == 1 then
		yield("/tp Estate Hall")
		yield("/wait 1")
		--yield("/waitaddon Nowloading <maxwait.15>")
		yield("/wait 15")
		yield("/waitaddon NamePlate <maxwait.600><wait.5>")
	end

	--normal small house shenanigans
	if chars_fn[rcuck_count][2] == 0 then
		yield("/hold W <wait.1.0>")
		yield("/release W")
		yield("/target Entrance <wait.1>")
		yield("/lockon on")
		yield("/automove on <wait.2.5>")
		yield("/automove off <wait.1.5>")
		yield("/hold Q <wait.2.0>")
		yield("/release Q")
	end

	--retainer bell nearby shenanigans
	if chars_fn[rcuck_count][2] == 1 then
		yield("/target \"Summoning Bell\"")
		yield("/wait 2")
		--PathfindAndMoveTo(GetObjectRawXPos("Summoning Bell"), GetObjectRawYPos("Summoning Bell"), GetObjectRawZPos("Summoning Bell"), false)
		WalkTo(GetObjectRawXPos("Summoning Bell"), GetObjectRawYPos("Summoning Bell"), GetObjectRawZPos("Summoning Bell"))
		visland_stop_moving() --added so we don't accidentally end before we get to the bell
	end
	
--[[ dumping out this part. opening venture coffers is kind of annoying waste of time. maybe we make it optional later -->TODO<--
	--Code for opening venture coffers
	yield("/wait 3")
	yield("/echo Number of Venture Coffers to open: "..GetItemCount(32161))
	VCnum = GetItemCount(32161)
	while (VCnum > 0) do
		--this is no longer reliable
		--yield("/item Venture Coffer")
		yield("/send X")
		yield("/wait 6")
		VCnum = GetItemCount(32161)
		yield("/echo Number of Venture Coffers left: "..GetItemCount(32161))
	end
	]]
	--yield("/autorun off")
	--Code for opening FC menu so allagan tools can pull the FC points
	yield("/freecompanycmd")
	yield("/wait 3")
end

--first turn on FC buffs
if process_fc_buffs == 1 then
	for i=1, #chars_FCBUFF do
		yield("/echo "..chars_FCBUFF[i][1])
		yield("/ays relog " ..chars_FCBUFF[i][1])
   	    yield("/echo 15 second wait")
	    yield("/wait 15")
		yield("/waitaddon NamePlate <maxwait.600><wait.10>")
		yield("/echo FC Seal Buff II")
		yield("/freecompanycmd <wait.1>")
		yield("/pcall FreeCompany false 0 4u <wait.1>")
		yield("/pcall FreeCompanyAction false 1 0u <wait.1>")
		yield("/pcall ContextMenu true 0 0 1u 0 0 <wait.1>")
		yield("/pcall SelectYesno true 0 <wait.1>")
	--Then you need to /pyes the "Execute"
	 end
	yield("/wait 3")
end

--gc turn in
if process_players == 1 then
	for i=1, #chars_fn do
	 yield("/echo Loading Characters for GC TURNIN -> "..chars_fn[i][1])
	 yield("/echo Processing Retainer Abuser "..i.."/"..#chars_fn)
	 yield("/ays relog " ..chars_fn[i][1])
	 --yield("/echo 15 second wait")
	yield("/wait 2")
	CharacterSafeWait()
	 yield("/echo Processing Retainer Abuser "..i.."/"..#chars_fn)
	TeleportToGCTown()
	ZoneTransition()
	WalkToGC()
	rcuck_count = i
	yield("/wait 2")
	Final_GC_Cleaning()
	end
end
--last one out turn off the lights
yield("/ays multi e")
yield("/pcraft stop")