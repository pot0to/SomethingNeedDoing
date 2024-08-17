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
Simpletweaks -> enable estate list
YesAlready -> /Enter .*/
Something Need Doing -> Turn off SND Targeting in options

Optional:
Autoretainer
Liza's plugin : Kitchen Sink if you want to use her queue method
Simpletweaks -> enable auto equip recommended

Known issues:
1. "Race Condition with trade windows"
Something i need to confirm and report -> Accounts, a, b, c, d.  Say i want to deliver from b,c,d to a, if i use pcall method they will keep trying until they finish delivering gil.  however
if i use the dropbox method I am 99% sure it will just kind of sit there thinking its processing a dropbox queue but in fact its just sitting there doing nothing if any of the trade windows are open
while other clients are trying and failing to open one with the char from account A.

2. if dropbox isnt on the item tab, nothing will work.

EXCEL helper:
="{"""&F2&"@"&P2&""""&", 0, 0},"


in this case. this is for robust gc turnin.  F is your column with first name last name e.g.  "goat fucker"  and P is your column with servername e.g. "Marilith"  so it will create text that looks like
{"Goat Fucker@Marilith", 0, 0},


which you can copy paste en masse, just remove the last comma from last row

for bagman its similar

="{"""&F2&"@"&P2&""""&", 0, 0,"&""""&R2&""""&"},"
]]

--Start because nobody read the instructions at the top <3
PandoraSetFeatureState("Auto-Fill Numeric Dialogs", false) 
--End because nobody read the instructions at the top <3

tonys_turf = "Maduin" --what server is tony on
tonys_spot = "Pavolis Meats" --where we tping to aka aetheryte name
tony_zoneID = 132 --this is the zone id for where the aetheryte is, if its anything other than 0, it will be evaluated to see if your already in teh zone for cases of multi transfer from or to same
tonys_house = 0 --0 fc 1 personal 2 apartment. don't judge. tony doesnt trust your bagman to come to the big house
tony_type = 0 --0 = specific aetheryte name, 1 first estate in list outside, 2 first estate in list inside
bagmans_take = 1000000 -- how much gil remaining should the bagma(e)n shave off the top for themselves?
tonyception = 0 --0 = dont be fancy, 1 = we have multiple fat tonies in the table and therefore we need to give 1 gil at the end of the trade so tony will leave and the next tony can come
bagman_type = 0 --[[
0 = pcalls (gil only, a bit sloppy too with no multi tony support), 
1 = dropbox with table config, 
2 = dropbox but all salvage and all but bagmans take of gil, 
3 = table config w bagman cut, 
4 = items are in the table itself instead of the item table. normal way is  
firstname lastname@server, x, x, tony name
now will be
firstname lastname@server, x, x, tony name, itemID, quantity
in this case it won't use the table at all
--]]

--[[
if all of these are not 42069420, then we will try to go there at the very end of the process otherwise we will go directly to fat tony himself
get yourself x y z this way if you want
yield("/echo "..GetPlayerRawXPos().." "..GetPlayerRawYPos().." "..GetPlayerRawZPos().."")
]]
tony_x = 42069420
tony_y = 42069420
tony_z = 42069420

--[[
how do i get an xyz?
run this:
yield("/echo "..GetPlayerRawXPos().." "..GetPlayerRawYPos().." "..GetPlayerRawZPos().."")
]]

--[[
BAGMAN firstnamelastname@server, meeting locationtype, returnhome 1 = yes 0 = no, 0 = fc entrance 1 = nearby bell, TONY firstnamelastname  (no server this time)
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

--[[
dropbox queue config template:

local filled_bags = {
{ItemID,Quantity},
{ItemID,Quantity},
{ItemID,Quantity}
}

notice lack of comma on last one. you can otherwise have as many items as you want in this list

you can set quantity higher than existing to ensure max out
1 = Gil
10155 = Ceruleum Fuel
10373 = Magitek Repair Mats
ill add more commonly used IDs here eventually dont wanna clutter too much though.
]]
local filled_bags = {
{1,999999999},
{2,999999999},
{3,999999999}
}

DropboxSetItemQuantity(1,false,0) --because we need to do this or shit breaks

for i=1, #filled_bags do
	filled_bags[i][3] = 1
end

function are_we_there_yet_jimmy()
	woah_bruv = 1
	for i=1, #filled_bags do
		if GetItemCount(filled_bags[i][1]) - filled_bags[i][2] > 0 then
			woah_bruv = 0
		end
	end
	return woah_bruv
end

loadfiyel = os.getenv("appdata").."\\XIVLauncher\\pluginConfigs\\SomethingNeedDoing\\_functions.lua"
functionsToLoad = loadfile(loadfiyel)
functionsToLoad()
DidWeLoadcorrectly()

--the boss wants that monthly gil payment, have your bagman ready with the gil. 
--If he has to come pick it up himself its gonna get messy

yield("/ays multi d")
fat_tony = "Firstname Lastname" --this is just a placeholder you dont have to technically set it.

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

get_to_the_choppa = 0 -- alternate exit var
horrible_counter_method = 0
windex = 1 --bagman index

local function shake_hands()
	get_to_the_choppa = 0
	horrible_counter_method = 0
	--get_to_the_choppa = 1 -- alternate exit var
--	while GetGil() > bagmans_take or get_to_the_choppa == 0 do
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
	while get_to_the_choppa == 0 do
		if (GetGil() < (bagmans_take + 1)) and (tony_type == 0 or tony_type == 2) then
			get_to_the_choppa = 1
		end
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
			--are_we_there_yet_jimmy() --setup exit conditions
			if bagman_type == 1 or bagman_type == 3 then
				for i=1, #filled_bags do
					yield("/echo attempting to add stuff to the bag....")
					DropboxSetItemQuantity(filled_bags[i][1],false,filled_bags[i][2])
					DropboxSetItemQuantity(filled_bags[i][1],true,filled_bags[i][2])
					yield("/wait 0.5")
				end
				horrible_counter_method = horrible_counter_method + 1
				yield("/echo DEBUG bagman type 1 processing....")
				if horrible_counter_method > 1 then
					get_to_the_choppa = 1
					yield("/echo DEBUG moving towards exiting bagman type 1....")
				end -- get out
			end
			if bagman_type == 2 or bagman_type == 3 then
				snaccman = GetGil() - bagmans_take
				if snaccman < 0 then
					snaccman = 0
				end
				if snaccman > 0 then
					DropboxSetItemQuantity(1,false,snaccman)
					yield("/echo here you go, all ... of...the...gil!")
				end
			end
			if bagman_type == 4 then
				yield("/echo attempting to add stuff to the bag....")
				DropboxSetItemQuantity(franchise_owners[windex][5],false,franchise_owners[windex][6])
				yield("/wait 0.5")
				horrible_counter_method = horrible_counter_method + 1
				yield("/echo DEBUG bagman type 4 processing....")
				if horrible_counter_method > 1 then
					get_to_the_choppa = 1
					yield("/echo DEBUG moving towards exiting bagman type 4....")
				end -- get out
			end
			if bagman_type == 2 then
				yield("/dropbox")
				yield("/wait 0.5")
				DropboxSetItemQuantity(22500,false,999999)
				DropboxSetItemQuantity(22501,false,999999)
				DropboxSetItemQuantity(22502,false,999999)
				DropboxSetItemQuantity(22503,false,999999)
				DropboxSetItemQuantity(22504,false,999999)
				DropboxSetItemQuantity(22505,false,999999)
				DropboxSetItemQuantity(22506,false,999999)
				DropboxSetItemQuantity(22507,false,999999)

				if GetItemCount(22500) == 0 and GetItemCount(22501) == 0 and GetItemCount(22502) == 0 and GetItemCount(22503) == 0 and GetItemCount(22504) == 0 and GetItemCount(22505) == 0 and GetItemCount(22506) == 0 and GetItemCount(22507) == 0 then
					if GetGil() == snaccman then
						get_to_the_choppa = 1
					end
				end
				horrible_counter_method = horrible_counter_method + 1
				yield("/echo DEBUG bagman type 2 processing....")
				if horrible_counter_method > 1 then
					get_to_the_choppa = 1
					yield("/echo DEBUG moving towards exiting bagman type 2....")
				end -- get out
			end		
		end
	end
		yield("/wait 1")
	--end
	if bagman_type > 0 then
	--get_to_the_choppa = 1 --so the loop exits
		yield("/wait 4")
		DropboxStart()
		yield("/echo DEBUG dropbox initiated")
		yield("/wait 2")
		floo = DropboxIsBusy()
		while floo == true do
		  floo = DropboxIsBusy()
		  yield("/wait 2")
		  yield("/echo Trading happening!")
		end
		yield("/wait 5")
		if tonyception == 1 then
			if bagman_type == 1 then
				for i=1, #filled_bags do
					yield("/echo cleaning this list out to 0 so we dont send multiple packages to same tony")
					DropboxSetItemQuantity(filled_bags[i][1],false,0)
					DropboxSetItemQuantity(filled_bags[i][1],true,0)
					yield("/wait 0.5")
				end
			end
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
			  --get_to_the_choppa = 1 --get out
			  yield("/wait 5")
		end
	end
end

for i=1,#franchise_owners do
	--update tony's name
	fat_tony = franchise_owners[i][4]
	windex = i
	
	yield("/echo Loading bagman to deliver protection payments Fat Tony -> "..fat_tony..".  Bagman -> "..franchise_owners[i][1])
	yield("/echo Processing Bagman "..i.."/"..#franchise_owners)

	--only switch chars if the bagman is changing. in some cases we are delivering to same tony or different tonies. we dont care about the numbers
	if GetCharacterName(true) ~= franchise_owners[i][1] then
		yield("/ays relog " ..franchise_owners[i][1])
		yield("/wait 2")
		CharacterSafeWait()
	end	

    yield("/echo Processing Bagman "..i.."/"..#franchise_owners)
	DropboxSetItemQuantity(1,false,0) --because we need to do this or shit breaks

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
	yield("GetGil() -> "..GetGil())
	yield("bagmans_take -> "..bagmans_take)
	--if GetGil() > bagmans_take then
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
				if tony_zoneID ~= GetZoneID() then --we are teleporting to Tony's spot
					yield("/tp "..tonys_spot)
					ZoneTransition()
				end
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
	--end
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