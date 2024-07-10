--needs bunch of stuff ill document later maybe
--put the sea pickles into chocobo
--turnoff addon errors

fartingGoat = 1

function turnin()
--turnin
yield("/target F'abodji")
yield("/wait 2")
yield("/interact")
yield("/wait 0.5")
yield("/send ESCAPE")
yield("/wait 0.5")

yield("/target F'abodji")
yield("/wait 2")
yield("/interact")
yield("/wait 0.5")
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

while fartingGoat == 1 do

--get more!
yield("/target Nahctahr")
yield("/wait 0.5")
yield("/interact")
yield("/wait 2")
if GetNodeText("GuildLeve", 5, 2) == "0" then
	yield("/echo itsa done")
	yield("/pcraft stop")
end
yield("/callback JournalDetail true 3 778")
yield("/callback JournalDetail true 3 779")
yield("/callback JournalDetail true 3 780")
yield("/callback JournalDetail true 3 781")
fartwait()
turnin()
yield("/target Nahctahr")
yield("/wait 0.5")
yield("/interact")
yield("/wait 2")
if GetNodeText("GuildLeve", 5, 2) == "0" then
	yield("/echo itsa done")
	yield("/pcraft stop")
end
yield("/callback JournalDetail true 3 779")
yield("/callback JournalDetail true 3 780")
yield("/callback JournalDetail true 3 781")
fartwait()
turnin()
yield("/target Nahctahr")
yield("/wait 0.5")
yield("/interact")
yield("/wait 2")
if GetNodeText("GuildLeve", 5, 2) == "0" then
	yield("/echo itsa done")
	yield("/pcraft stop")
end
yield("/callback JournalDetail true 3 780")
yield("/callback JournalDetail true 3 781")
fartwait()
turnin()
yield("/target Nahctahr")
yield("/wait 0.5")
yield("/interact")
yield("/wait 2")
yield("/callback JournalDetail true 3 781")
yield("/wait 0.5")
yield("/send ESCAPE")
yield("/wait 0.5")
yield("/send ESCAPE")
end