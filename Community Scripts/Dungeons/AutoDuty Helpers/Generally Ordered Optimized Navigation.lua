--[[
Generally Ordered Optimized Navigation.lua  (thanks for the idea fifi)
or "Something need Gooning"
thanks to @Akasha and @Ritsuko for some of the ideas/code

purpose: help autoduty with farming duties.

Plugins/configs (ill update as people realize i forgot instructions)
Automaton
Some form of bossmod
Rotation Solver Reborn
Vnavmesh
Pandora -> actually have this disabled it causes problems.
Something Need doing (SND)
Simpletweaks
and more (?)
Simpletweaks -> targeting fix
SND -> disable snd targeting
SND -> disable addon errors

Yesalready configs (maybe only the first one is needed since the rest are done via callbacks w ya off) also make sure yesalready is on :p ad turns it off sometimes (???)
	"YesNo"
		Return to the starting point for the Praetorium?   ※You may be unable to re-enter ongoing battles.
		/Repair all displayed items for.*/
		/Exit.*/
	"Lists"
		/Retire to an inn room.*/

Enhanced Duty start/end
	duty start -> /pcraft run start_gooning
	duty end -> /ad stop
	leave duty -> 10 seconds
Use whatever path you want. but i reccommend the included path file for tank and others. Credit to @Akasha and @Ritsuko for the path files.  tank tries to w2w, tank2 does not.

todo:
duty specific stuff like only do the phantom targetioong for gaeuis in prae

known issues:
I SAW AN UNKNOWN LUA ERROR so i might add debug lines everywhere until i catch it.

recommended party:
war dps dps sch
for sch in RSR turn off adloquim, succor and physick

--]]
yield("/echo please get ready for G.O.O.N ing time")

jigglecounter = 0
x1 = GetPlayerRawXPos()
y1 = GetPlayerRawYPos()
z1 = GetPlayerRawZPos()

stopcuckingme = 0    --counter for checking whento pop duty

--if its a cross world party everyoner will make a queue attempt
function isLeader()
    return (GetCharacterName() == GetPartyMemberName(GetPartyLeadIndex()))
end

imthecaptainnow = 0  --set this to 1 if its the party leader

if isLeader() then 
	imthecaptainnow = 1 
	yield("/echo I am the party leader i guess")
end

maxjiggle = 6 -- = how much time before we jiggle
while 1 == 1 do
--safe check ifs
if IsPlayerAvailable() then
if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
--
	--is there some bullshit and yesalready was disabled?
	yield("/callback SelectYesno true 0")
	--Do we need repairs?
	--check every 0.5 seconds 8 times so total looop is 5 seconds
	goat = 0
	while goat < 9 do
		goat = goat + 1
		yield("/wait 0.5")
		if GetCharacterCondition(34) == false then
			--SELF REPAIR
			local minicounter = 0
			--check if we even have g8dm, otherwise dont waste time, 10386 is g6dm if you wanna change it, 17837 is g7, 33916 is g8
			if GetItemCount(33916) > 0 then
				if NeedsRepair(99) then
					yield("/wait 10")
					while not IsAddonVisible("Repair") do
					  yield("/generalaction repair")
					  yield("/wait 1")
					  minicounter = minicounter + 1
					  if minicounter > 20 then
						minicounter = 0
						break
					  end
					end
					yield("/callback Repair true 0")
					yield("/wait 0.1")
					if IsAddonVisible("SelectYesno") then
					  yield("/callback SelectYesno true 0")
					  yield("/wait 1")
					end
					while GetCharacterCondition(39) do yield("/wait 1")
					yield("/wait 1")
					yield("/callback Repair true -1")
					  minicounter = minicounter + 1
					  if minicounter > 20 then
						minicounter = 0
						break
					  end
					end
				end
			end
			--JUST OUTSIDE THE INN REPAIR
			if NeedsRepair(50) and GetItemCount(1) > 4999 and GetCharacterCondition(34) == false then --only do this outside of a duty yo
				yield("/ad repair")
				goatcounter = 0
				for goatcounter=1,30 do
					yield("/wait 0.5")
					yield("/callback _Notification true 0 17")
					yield("/callback ContentsFinderConfirm true 9")
				end

			end
		end
		--reenter the inn room
		--if (GetZoneID() ~= 177 and GetZoneID() ~= 178) and GetCharacterCondition(34) == false and NeedsRepair(50) == false then
		if (GetZoneID() ~= 177 and GetZoneID() ~= 178) and GetCharacterCondition(34) == false then
			yield("/send ESCAPE")
			yield("/ad stop") --seems to be needed or we get stuck in repair genjutsu
			yield("/target Antoinaut") --gridania
			yield("/target Mytesyn")   --limsa
			yield("/target Otopa")     --uldah
			yield("/wait 1")
			yield("/lockon on")
			yield("/automove")
			yield("/wait 2")
			yield("/wait 0.5")
			yield("/callback _Notification true 0 17")
			yield("/callback ContentsFinderConfirm true 9")
			yield("/interact")
			yield("/wait 1")
			yield("/callback _Notification true 0 17")
			yield("/callback ContentsFinderConfirm true 9")
			yield("/callback SelectIconString true 0")
			yield("/callback _Notification true 0 17")
			yield("/callback ContentsFinderConfirm true 9")
			yield("/callback SelectString true 0")
			yield("/wait 1")
			--yield("/wait 8")
			--RestoreYesAlready()
		end
	end
	--end safe check one
	end
	end
	--
	--safe check ifs part 2
	if IsPlayerAvailable() then
	if type(GetCharacterCondition(34)) == "boolean" and type(GetCharacterCondition(26)) == "boolean" and type(GetCharacterCondition(4)) == "boolean" then
	--

	--yield("/echo x diff"..math.abs(x1 - GetPlayerRawXPos()))
	--check if we are stuck somewhere.
	--first ensure we are in the duty and not in combat

	if GetZoneID() == 1044 and GetCharacterCondition(26) == false then --Praetorium
		maxjiggle = 6
		flurb = "????"
		for flurby = 1,30 do
			if GetNodeText("_ToDoList", flurby, 3) == "Arrive at the command chamber: 0/1" then flurb = "Arrive at the command chamber: 0/1" end
			if GetNodeText("_ToDoList", flurby, 3) == "Clear the command chamber: 0/1" then flurb = "Clear the command chamber: 0/1" end
			if GetNodeText("_ToDoList", flurby, 3) == "Arrive at the Laboratorium Primum: 0/1" then flurb = "Arrive at the Laboratorium Primum: 0/1" end
			if GetNodeText("_ToDoList", flurby, 3) == "Clear the Laboratorium Primum: 0/1" then flurb = "Clear the Laboratorium Primum: 0/1" end
			if GetNodeText("_ToDoList", flurby, 3) == "Arrive on the Echelon: 0/1" then flurb = "Arrive on the Echelon: 0/1" end
			if GetNodeText("_ToDoList", flurby, 3) == "Defeat Gaius van Baelsar: 0/1" then flurb = "Defeat Gaius van Baelsar: 0/1" end
			yield("/wait 0.1")
		end
		if flurb == "Clear the Laboratorium Primum: 0/1"  and GetCharacterCondition(26) == false then
			flurb = GetNodeText("_ToDoList", 25, 3)
--this doesnt work the way i intended so removing it for now.
			--[[yield("/target Shortcut")
			yield("/wait 0.5")
			yield("/target Nero")
			yield("/wait 0.5")
			if type(GetTargetName()) == "string" and GetTargetName() == "Shortcut" then
				yield("/ad stop")
				yield("/interact")
				yield("/vnavmesh moveto "..GetTargetRawXPos().." "..GetTargetRawYPos().." "..GetTargetRawZPos())
				yield("/wait 10")
				yield("/interact")
				yield("/bmrai on")
				yield("/rotation auto")
			end
			if type(GetTargetName()) == "string" and GetCharacterCondition(26) == false then
				yield("/vnavmesh moveto "..GetTargetRawXPos().." "..GetTargetRawYPos().." "..GetTargetRawZPos())
			end
			--]]
		end
		if flurb == "Arrive on the Echelon: 0/1"  and GetCharacterCondition(26) == false then
			maxjiggle = 20
		end
		if flurb == "Defeat Gaius van Baelsar: 0/1" and GetCharacterCondition(26) == false then
			maxjiggle = 20
			yield("/target Magitek")
			yield("/wait 0.5")
			yield("/target Shortcut")
			yield("/wait 0.5")
			yield("/interact")
			yield("/hold W <wait.2.0>")
			yield("/release W")
			yield("/interact")
			if type(GetTargetName()) == "string" and GetTargetName() == "Shortcut" then
				yield("/ad stop")
				yield("/interact")
				yield("/vnavmesh moveto "..GetTargetRawXPos().." "..GetTargetRawYPos().." "..GetTargetRawZPos())
				yield("/wait 10")
				yield("/interact")
				yield("/bmrai on")
				yield("/rotation auto")
			end
			if type(GetTargetName()) == "string" and GetCharacterCondition(26) == false then
				yield("/interact")
				yield("/vnavmesh moveto "..GetTargetRawXPos().." "..GetTargetRawYPos().." "..GetTargetRawZPos())
			end
			yield("/target Gauis")
			yield("/wait 0.5")
		end
		yield("/echo Prae Duty Progress -> "..flurb)
	end

	--1044 is prae we only need this there atm
	if GetZoneID() == 1044 then --Praetorium
	if GetCharacterCondition(34) == true and GetCharacterCondition(26) == false then
		if math.abs(x1 - GetPlayerRawXPos()) < 3 and math.abs(y1 - GetPlayerRawYPos()) < 3 and math.abs(z1 - GetPlayerRawZPos()) < 3 then
			yield("/echo we havent moved very much something is up ")
			jigglecounter = jigglecounter + 1
		end
		if jigglecounter > maxjiggle then --we stuck for 30+ seconds somewhere
			yield("/echo attempting to restart AD and hope for the best")
			jigglecounter = 0
			yield("/ad stop")
			yield("/wait 2")
			yield("/return")
			yield("/wait 1")
			yield("/callback SelectYesno true 0")
			yield("/wait 12")
			yield("/ad start")
			yield("/wait 2")
		end
	end

		local mytarget = GetTargetName()
		if type(mytarget) == "string" and mytarget ~= "Phantom Gaius" then
			local ndist = GetDistanceToObject(null)
			local gdist = GetDistanceToObject("Phantom Gaius")
			local deltadist = ndist - gdist
			if (deltadist > 1 or deltadist < -1) and gdist < 100 then
				yield("/echo target")
				TargetClosestEnemy()
			end
		end
	end
	--if GetCharacterCondition(34) == false then --fix autoqueue just shitting out
		--yield("/send U")
	--end
	
	if GetCharacterCondition(34) == true and GetCharacterCondition(26) == false then
		yield("/equiprecommended")
		TargetClosestEnemy()
	end
	
	if GetCharacterCondition(4) == false and GetCharacterCondition(26) == true then
		yield("/vnav stop")
		jigglecounter = 0 -- we reset the jiggle counter while we are in combat. combat is good means we are doing something productive
		yield("/echo stopping nav for combat")
	end
	
	if GetCharacterCondition(34) == true then
		x1 = GetPlayerRawXPos()
		y1 = GetPlayerRawYPos()
		z1 = GetPlayerRawZPos()
	end
    yield("/wait 1.0")

	stopcuckingme = stopcuckingme + 1
	--autoqueue at the end because its least important thing
	if stopcuckingme > 2 and GetCharacterCondition(34) == false and imthecaptainnow == 1 then
		yield("/finder")
		yield("/echo attempting to trigger duty finder")
		yield("/callback ContentsFinder true 12 0")
		stopcuckingme = 0
	end

--safe check ends
end
end
---
end