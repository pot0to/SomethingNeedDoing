--[[
Have you ever wanted your alts that have been making gil for you on other accounts, to deliver that gil TO you?
well this script (will eventually) rotate through your alts, and visit a server and or place to deliver gil.

requires plugins
Lifestream
Teleporter
Pandora -> TURN OFF AUTO NUMERICS
automaton -> TURN OFF AUTO NUMERICS
Dropbox -> autoconfirm
Visland
Vnavmesh
Simpletweaks -> enable targeting fix
YesAlready -> /Enter .*/

Optional:
Autoretainer
Liza's plugin : Kitchen Sink if you want to use her queue method

]]

--Start because nobody read the instructions at the top <3
PandoraSetFeatureState("Auto-Fill Numeric Dialogs", false) 
--End because nobody read the instructions at the top <3

fat_tony = "Firstname Lastname" --what is the name of the destination player who will receive the gil
tonys_turf = "Maduin" --what server is tony on
tonys_spot = "Pavolis Meats" --where we tping to aka aetheryte name
tonys_house = 0 --0 fc 1 personal 2 apartment. don't judge. tony doesnt trust your bagman to come to the big house
tony_type = 1 --0 = specific aetheryte name, 1 first estate in list outside, 2 first estate in list inside
bagmans_take = 1000000 -- how much gil remaining should the bagma(e)n shave off the top for themselves?
bagman_type = 0 --0 = pcalls, 1 = dropbox with table config, 2 = dropbox but all salvage and all but bagmans take of gil
tonyception = 0 --0 = dont be fancy, 1 = we have multiple fat tonies in the table and therefore we need to give 1 gil at the end of the trade so tony will leave and the next tony can come

--if all of these are not 42069420, then we will try to go there at the very end of the process otherwise we will go directly to fat tony himself
tony_x = 42069420
tony_y = 42069420
tony_z = 42069420

--[[
how do i get an xyz?
run this:
yield("/echo "..GetPlayerRawXPos().." "..GetPlayerRawYPos().." "..GetPlayerRawZPos().."")
]]


--[[
tony firstnamelastname@server, meeting locationtype, returnhome 1 = yes 0 = no, 0 = fc entrance 1 = nearby bell, BAGMAN firstnamelastname  (no server this time)
]]

local franchise_owners = {
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"},
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"},
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"},
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"},
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"},
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"},
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"},
{"Firstname Lastname@Server", 1, 0, "Firstname Lastname"}
}

--dropbox queue config,   ItemID,Quantity
--you can set quantity higher than existing to ensure max out
local filled_bags = {
{1,999999999},
{2,999999999},
{3,999999999}
}

loadfiyel = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\_functions.lua"
functionsToLoad = loadfile(loadfiyel)
functionsToLoad()
DidWeLoadcorrectly()

--the boss wants that monthly gil payment, have your bagman ready with the gil. 
--If he has to come pick it up himself its gonna get messy

yield("/ays multi d")
snagmanstake = bagmanstake

local function distance(x1, y1, z1, x2, y2, z2)
	if type(x1) ~= "number" then x1 = 0 end
	if type(y1) ~= "number" then y1 = 0 end
	if type(z1) ~= "number" then z1 = 0 end
	if type(x2) ~= "number" then x2 = 0 end
	if type(y2) ~= "number" then y2	= 0 end
	if type(z2) ~= "number" then z2 = 0 end
	zoobz = math.sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
	if type(zoobz) ~= "number" then
		zoobz = 0
	end
    --return math.sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
    return zoobz
end

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
	if GetGil() > bagmans_take then
		thebag = GetGil() - bagmans_take
		if thebag < 0 then
			thebag = GetGil()
		end

		--loop until we have tony targeted
		yield("/target "..fat_tony)
		yield("/wait 1")
		while string.len(GetTargetName()) == 0 do
			yield("/target "..fat_tony)
			yield("/wait 1")
		end
		
		--we got fat tony.  we just need to make sure he is within targeting distance. say <1 yalms before we continue
		while distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fat_tony),GetObjectRawYPos(fat_tony),GetObjectRawZPos(fat_tony)) > 1.5 do
			yield("/echo this fat bastard better hurry up he is  "..distance(GetPlayerRawXPos(), GetPlayerRawYPos(), GetPlayerRawZPos(), GetObjectRawXPos(fat_tony),GetObjectRawYPos(fat_tony),GetObjectRawZPos(fat_tony)).." away!")
			yield("/target "..fat_tony)   --just in case we had wrong "tony" targeted
			yield("/wait 1")
		end
		
		--DEBUG
		--yield("/echo our return mode will be "..franchise_owners[1][2])
		
		--pcall way to transfer gil only. we can't pcall other methods
		while GetGil() > bagmans_take do
			yield("/target "..fat_tony)
			yield("/echo here you go "..fat_tony..", another full bag, with respect")
			if bagman_type == 0 then
				yield("/trade")
				yield("/wait 0.5")
				yield("/wait 0.5")
				yield("/pcall Trade true 2")
				--verification of target before doing the following. otherwise hit escape!
				tradename = GetNodeText("Trade", 20)
				if tradename ~= fat_tony then
					--we got someone with their hand in the till. we'll send them a fish wrapped in newspaper later
					ungabunga()
				end
				if tradename == fat_tony then
					if GetGil() > 999999 then
						yield("/pcall InputNumeric true 1000000 <wait.1>") --this is just in case we want to specify/calculate the amount
					end
					if GetGil() < 1000000 then
						snaccman = GetGil() - bagmans_take
						yield("/pcall InputNumeric true ".. snaccman .." <wait.1>") --this is just in case we want to specify/calculate the amount
					end
					yield("/pcall Trade true 0")
					yield("/wait 4")
				end
			end
			if bagman_type > 0 then 
				yield("/dropbox")
				yield("/wait 1")
				yield("/focustarget <t>")
				yield("/wait 0.5")
				if bagman_type == 1 then
					for i=1, #filled_bags do
						--yield("/dbq "..filled_bags[i][1]..":"..filled_bags[i][2])  --kitchensync seems to immediately start trade would have to format this as a long concatenated string instead
						DropboxSetItemQuantity(filled_bags[i][1],false,filled_bags[i][2])
					end
				end
				if bagman_type == 2 then
					snaccman = GetGil() - bagmans_take
					yield("/dropbox")
					yield("/wait 0.5")
					if snaccman > 0 then
						--yield("/dbq 1:"..snaccman) -- we can't rob tony.. yet
						DropboxSetItemQuantity(1,false,snaccman)
					end					
					DropboxSetItemQuantity(22500,false,999999)
					DropboxSetItemQuantity(22501,false,999999)
					DropboxSetItemQuantity(22502,false,999999)
					DropboxSetItemQuantity(22503,false,999999)
					DropboxSetItemQuantity(22504,false,999999)
					DropboxSetItemQuantity(22505,false,999999)
					DropboxSetItemQuantity(22506,false,999999)
					DropboxSetItemQuantity(22507,false,999999)
					--[[
					yield("/dbq 22500:*")  --  22500  Salvaged
					yield("/dbq 22501:*")  --  22501  Salvaged
					yield("/dbq 22502:*")  --  22502  Salvaged
					yield("/dbq 22503:*")  --  22503  Salvaged
					yield("/dbq 22504:*")  --  22504  Salvaged
					yield("/dbq 22505:*")  --  22505  Salvaged
					yield("/dbq 22506:*")  --  22506  Salvaged
					yield("/dbq 22507:*")  --  22507  Salvaged
					]]
				end
				
				bagmans_take = 9999999999 --so the loop exits
				yield("/wait 4")
				DropboxStart()
				yield("/wait 2")
 			    floo = DropboxIsBusy()
				while floo == true do
				  floo = DropboxIsBusy()
				  yield("/wait 2")
				  yield("/echo Trading happening!")
				end
				yield("/wait 5")
				if tonyception == 1 then
				  yield("/echo Woah we need another tony out here im not giving you this next bag you mook")
				  --yield("/dbq 1:1") 
				  DropboxSetItemQuantity(1,false,1)
  				  DropboxStart()
				  yield("/wait 2")
 			      floo = DropboxIsBusy()
				  while floo == true do
					  floo = DropboxIsBusy()
					  yield("/wait 2")
					  yield("/echo Trading happening!!")
				  end
				  yield("/wait 5")
				end
			end
			yield("/wait 1")
		end
	end
end

for i=1,#franchise_owners do
	--update tony's name
	fat_tony = franchise_owners[i][4]
	
	yield("/echo Loading bagman to deliver protection payments Fat Tony -> "..fat_tony..".  Bagman -> "..franchise_owners[i][1])
	yield("/echo Processing Bagman "..i.."/"..#franchise_owners)

	--only switch chars if the bagman is changing. in some cases we are delivering to same tony or different tonies. we dont care about the numbers
	if GetCharacterName(true) ~= franchise_owners[i][1] then
		yield("/ays relog " ..franchise_owners[i][1])
		yield("/wait 2")
		CharacterSafeWait()
	end	

    yield("/echo Processing Bagman "..i.."/"..#franchise_owners)

	--AGP. always get paid.
	--don't deliver if we can't pay ourselves. Tony is too lazy and stupid to come check our franchise anyways.
	--tell him our grandmother was sick
	if GetGil() < bagmans_take then
		yield("/echo Maybe "..fat_tony.." won't notice we didn't pay this month?")
		yield("/echo also yo, you out there watching this. why did you include this char in the list are you lazy?")
		yield("/wait 5")
	end
	
	--allright time for a road trip. let get that bag to Tony
	road_trip = 0
	if GetGil() > bagmans_take then
		road_trip = 1 --we took a road trip
		--now we must head to fat_tony 
		--first we have to find his neighbourhood, this uber driver better not complain
		--are we on the right server already?
		yield("/li "..tonys_turf)
		yield("/wait 15")
		CharacterSafeWait()
		yield("/echo Processing Bagman "..i.."/"..#franchise_owners)
		
		--now we have to walk or teleport?!!?!? to fat tony, where is he waiting this time?
		if tony_type == 0 then
			yield("/echo "..fat_tony.." is meeting us in the alleyways.. watch your back")
			yield("/tp "..tonys_spot)
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
		bagmans_take = snagmanstake
	end
	if road_trip == 1 then --we need to get home
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
			--limsa bell
			if franchise_owners[i][3] == 2 then
				return_to_limsa_bell()
			end
		end
	end
end

--what you thought your job was done you ugly mug? get back to work you gotta pay up that gil again next month!
yield("/ays multi e")
