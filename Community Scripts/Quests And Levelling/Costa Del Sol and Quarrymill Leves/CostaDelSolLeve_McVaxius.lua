--needs bunch of stuff ill document later maybe
--put the sea pickles into chocobo
--turnoff addon errors

function visland_stop_moving()
 yield("/equipguud")
 yield("/equiprecommended")
 yield("/character")
 yield("/pcall Character true 15")
 yield("/wait 0.5")
 yield("/pcall SelectYesno true 0")
 yield("/character")
 yield("/pcall Character true 15")
 yield("/pcall SelectYesno true 0")
 yield("/wait 3")
 muuv = 1
 muuvX = GetPlayerRawXPos()
 muuvY = GetPlayerRawYPos()
 muuvZ = GetPlayerRawZPos()
 while muuv == 1 do
	yield("/wait 1")
	if muuvX == GetPlayerRawXPos() and muuvY == GetPlayerRawYPos() and muuvZ == GetPlayerRawZPos() then
		muuv = 0
	end
	muuvX = GetPlayerRawXPos()
	muuvY = GetPlayerRawYPos()
	muuvZ = GetPlayerRawZPos()
 end
 yield("/echo movement stopped safely - script proceeding to next bit")
 yield("/visland stop")
 yield("/vnavmesh stop")
 yield("/wait 3")
end

yield("/vnavmesh moveto 453.40570068359 17.503484725952 475.26538085938")
visland_stop_moving()

fartingGoat = 1

function turnin()
	hehehe = 1
	yield("/echo Quest list -> "..GetNodeText("_ToDoList", 8, 13))
	
	while hehehe == 1 do
		hehehe = 0
		floob = GetNodeText("_ToDoList", 8, 13)
		if floob == "A Recipe for Disaster" or floob == "Just Call Me Late for Dinner" or floob == "Kitchen Nightmares No More" then
			hehehe = 1
		end
		floob = GetNodeText("_ToDoList", 8, 13)
		yield("/target F'abodji")
		yield("/wait 0.5")
		yield("/interact")
		yield("/wait 1")
	end

	yield("/send ESCAPE")
	yield("/wait 0.5")
end

function fartwait()
	yield("/send ESCAPE")
	yield("/wait 0.5")
	yield("/send ESCAPE")
	yield("/wait 0.5")
	yield("/send ESCAPE")
	yield("/wait 2")
end

function checkingu_node()
	yield("/echo Node Text -> "..GetNodeText("GuildLeve", 11, 40, 4))
	if GetNodeText("GuildLeve", 11, 40, 4) == "A Recipe for Disaster" then
		yield("/callback JournalDetail true 3 778")
	end
	if GetNodeText("GuildLeve", 11, 40, 4) == "Just Call Me Late for Dinner" then
		yield("/callback JournalDetail true 3 779")
	end
	if GetNodeText("GuildLeve", 11, 40, 4) == "Kitchen Nightmares No More" then
		yield("/callback JournalDetail true 3 780")
	end
	if GetNodeText("GuildLeve", 11, 40, 4) == "The Blue Period" then
		yield("/callback JournalDetail true 3 781")
	end
end

while fartingGoat == 1 do
	yield("/target Nahctahr")
	yield("/wait 0.5")
	yield("/interact")
	yield("/wait 3")
	if GetNodeText("GuildLeve", 5, 2) == "0" then
		yield("/echo itsa done")
		yield("/pcraft stop")
	end
	weew = 8
	oldnode = "wow amazing"
	while weew > 0 do
		if oldnode == GetNodeText("GuildLeve", 11, 40, 4) then
			weew = 0
		end
		checkingu_node()
		weew = weew - 1
		yield("/wait 1")
		if IsAddonVisible("GuildLeve") == false then
			weeew = 0
			fartwait()
		end
		oldnode = GetNodeText("GuildLeve", 11, 40, 4)
	end
	turnin()
end