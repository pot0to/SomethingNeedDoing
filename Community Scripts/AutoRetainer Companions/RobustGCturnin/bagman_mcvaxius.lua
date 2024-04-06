--[[
Have you ever wanted your alts that have been making gil for you on other accounts, to deliver that gil TO you?
well this script (will eventually) rotate through your alts, and visit a server and or place to deliver gil.

requires plugins
Lifestream
Teleporter
Pandora
Visland
Vnavmesh
Simpletweaks -> enable targeting fix
YesAlready -> /Enter .*/

Optional:
Autoretainer

]]

fat_tony = "Firstname Lastname" --what is the name of the destination player who will receive the gil
tonys_turf = "Maduin" --what server is tony on
tonys_spot = "Pavolis Meats" --where we tping to aka aetheryte name
tonys_house = 0 --0 fc 1 personal 2 apartment. don't judge. tony doesnt trust your bagman to come to the big house
tony_type = 1 --0 = specific aetheryte name, 1 first estate in list outside, 2 first estate in list inside
bagmans_take = 1000000 -- how much gil remaining should the bagma(e)n shave off the top for themselves?

--if all of these are not 42069420, then we will try to go there at the very end of the process otherwise we will go directly to fat tony himself
tony_x = 42069420
tony_y = 42069420
tony_z = 42069420

--[[
firstname, lastname, meeting locationtype, returnhome 1 = yes 0 = no, 0 = fc entrance 1 = nearby bell
]]

local franchise_owners = {
{"Firstname Lastname@Server", 1, 0},
{"Firstname Lastname@Server", 1, 0},
{"Firstname Lastname@Server", 1, 0},
{"Firstname Lastname@Server", 1, 0},
{"Firstname Lastname@Server", 1, 0},
{"Firstname Lastname@Server", 1, 0},
{"Firstname Lastname@Server", 1, 0},
{"Firstname Lastname@Server", 1, 0}
}

loadfiyel = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\_functions.lua"
functionsToLoad = loadfile(loadfiyel)
functionsToLoad()
DidWeLoadcorrectly()

--the boss wants that monthly gil payment, have your bagman ready with the gil. 
--If he has to come pick it up himself its gonna get messy

yield("/ays multi d")

local function approach_tony()
	local specific_tony = 0
	if tony_x ~= 42069420 and tony_y ~= 42069420 and tony_z ~= 42069420 then
		specific_tony = 1
	end
	if specific_tony == 0 then
		PathfindAndMoveTo(GetObjectRawXPos(fat_tony),GetObjectRawYPos(fat_tony),GetObjectRawZPos(fat_tony), false)
	end
	if specific_tony == 1 then
		PathfindAndMoveTo(tony_x,tony_y,tony_z, false)
	end
end

local function approach_entrance()
	PathfindAndMoveTo(GetObjectRawXPos("Entrance"),GetObjectRawYPos("Entrance"),GetObjectRawZPos("Entrance"), false)
end

local function shake_hands()
	if GetGil() == 0 then
		yield("what are you stupid? you better run before "..fat_tony.." realizes you tricked them")
	end
	if GetGil() > 0 then
		thebag = GetGil() - bagmans_take
		if thebag < 0 then
			thebag = GetGil()
		end
		yield("/target "..fat_tony)
		yield("/wait 1")
		--*/dropbox trade gil thebag
		--*some kind of loop to check gil amount until it reaches the desired remainder
	end
end

for i=1,#franchise_owners do
	 yield("/echo Loading bagman to deliver protection payments Fat Tony -> "..fat_tony..".  Bagman -> "..franchise_owners[i][1])
	 yield("/echo Processing Bagman "..i.."/"..#franchise_owners)
	 yield("/ays relog " ..franchise_owners[i][1])
	yield("/wait 2")
	CharacterSafeWait()
    yield("/echo Processing Bagman "..i.."/"..#franchise_owners)
	
	--now we must head to fat_tony 
	--first we have to find his neighbourhood, this uber drive better not complain
	--are we on the right server already?
	yield("/li "..tonys_turf)
	yield("/wait 15")
	CharacterSafeWait()
    yield("/echo Processing Bagman "..i.."/"..#franchise_owners)
	
	--now we have to walk or teleport?!!?!? to fat tony, where is he waiting this time?
	if tony_type == 0 then
		yield("/echo "..fat_tony.." is meeting us in the alleyways.. watch your back")
		yield("/tp \"tonys_spot\"")
		ZoneTransition()
	end
	if tony_type > 0 then
		yield("/echo "..fat_tony.." is meeting us at the estate, we will approach with respect")
		yield("/estatelist "..fat_tony)
		yield("/wait 0.5")
		--very interesting discovery
		--1= personal, 0 = fc, 2 = apartment
		yield("/pcall TeleportHousingFriend true "..tonys_house)
		ZoneTransition()
	end
	
	--ok tony is nearby. let's approach this guy, weapons sheathed, we are just doing business
	if tony_type == 0 then
		approach_tony()
		visland_stop_moving()
	end
	if tony_type == 1 then
		approach_entrance()
		visland_stop_moving()
		if tony_type == 2 then
			yield("/interact")
			yield("/pcall SelectYesNo true 0")  --this doesnt work. just use yesalready. putting it here for later in case someone else sorts it out i can update.
			yield("/wait 5")
		end
		approach_tony()
		visland_stop_moving()
	end
	shake_hands() -- its a business doing pleasure with you tony as always
	--time to go home.. maybe?
	if franchise_owners[i][2] == 0 then
		yield("/echo wait why can't i leave "..fat_tony.."?")
	end
	if franchise_owners[i][2] == 1 then
		yield("/li")
		yield("/echo See ya "..fat_tony..", a pleasure.")
		yield("/wait 5")
		CharacterSafeWait()
		--added 5 second wait here because sometimes they get stuck.
		yield("/wait 5")
		yield("/tp Estate Hall")
		yield("/wait 1")
		--yield("/waitaddon Nowloading <maxwait.15>")
		yield("/wait 15")
		yield("/waitaddon NamePlate <maxwait.600><wait.5>")
		--normal small house shenanigans
		if franchise_owners[i][3] == 0 then
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
		if franchise_owners[i][3] == 1 then
			yield("/target \"Summoning Bell\"")
			yield("/wait 2")
			PathfindAndMoveTo(GetObjectRawXPos("Summoning Bell"), GetObjectRawYPos("Summoning Bell"), GetObjectRawZPos("Summoning Bell"), false)
			visland_stop_moving() --added so we don't accidentally end before we get to the bell
		end
	end
end

--what you thought your job was done you ugly mug? get back to work you gotta pay up that gil again next month!
yield("/ays multi e")
