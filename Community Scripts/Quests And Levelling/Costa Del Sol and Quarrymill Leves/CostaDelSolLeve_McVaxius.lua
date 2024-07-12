--needs bunch of stuff ill document later maybe
--put the sea pickles into chocobo
--turnoff addon errors

fartingGoat = 1

function turnin()
	--turnin
	yield("/target F'abodji")
	yield("/wait 1")
	yield("/interact")
	yield("/wait 3")

	yield("/target F'abodji")
	yield("/wait 1")
	yield("/interact")
	yield("/wait 3")

	yield("/target F'abodji")
	yield("/wait 1")
	yield("/interact")
	yield("/wait 3")
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
	weew = 4
	oldnode = "wow amazing"
	while weew > 0 do
		if oldnode == GetNodeText("GuildLeve", 11, 40, 4) then
			weew = 0
		end
		checkingu_node()
		weew = weew - 1
		yield("/wait 2")
		if IsAddonVisible("GuildLeve") == false then
			weeew = 0
			fartwait()
		end
		oldnode = GetNodeText("GuildLeve", 11, 40, 4)
	end
	turnin()
end