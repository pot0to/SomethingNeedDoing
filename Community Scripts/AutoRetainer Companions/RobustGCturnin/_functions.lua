function become_feesher()
	yield("/equipjob fsh")
	yield("/equipitem 2571") --weathered fishing rod
	yield("/equipitem 35393") --weathered fishing rod
end

function ungabunga()
	yield("/send ESCAPE <wait.1.5>")
	yield("/send ESCAPE <wait.1.5>")
	yield("/send ESCAPE <wait.1.5>")
	yield("/send ESCAPE <wait.1>")
	yield("/wait 3")
end

function ungabungabunga()
	tobungaorunga = 0
	while tobungaorunga == 0 do
		yield("/send ESCAPE <wait.1.5>")
		yield("/send ESCAPE <wait.1.5>")
		yield("/send ESCAPE <wait.1.5>")
		yield("/send ESCAPE <wait.1>")
		yield("/wait 3")
		if IsPlayerAvailable() == true then
			tobungaorunga = 1
		end
	end
end

function generateRandomLetter(cappy)
    local uppercase = math.random(65, 90) -- ASCII codes for uppercase letters
    local lowercase = math.random(97, 122) -- ASCII codes for lowercase letters
    local choice = math.random(0, 1) -- Randomly choose between uppercase and lowercase
	if cappy == 2 then choice = 0 end
	if cappy == 3 then choice = 1 end
    if choice == 0 then
        return string.char(lowercase)
    else
        return string.char(uppercase)
    end
end

--0=no, 1=full randomize, 2=lowercase only, 3=uppercase only, 4=randomly full upper OR lowercase, 5=pick from emblem configuration list. remember this has to be the FC leader
function generateFiveDigitText(frocess_tags)
    local text = ""
	capper = frocess_tags
	if capper == 4 then
		local choice = math.random(0, 1) -- Randomly choose between uppercase and lowercase
		choice = choice + 2
	end
    for i = 1, 5 do
        text = text .. generateRandomLetter(capper)
    end
    return text
end

function WalkTo(x, y, z)
	PathfindAndMoveTo(x, y, z, false)
	countee = 0
	while (PathIsRunning() or PathfindInProgress()) do
		yield("/wait 0.5")
		--if GetZoneID() == 130 or GetZoneID() == 341 then --130 is uldah. dont need to jump anymore it paths properly. we will test anyways.
		countee = countee + 1
		if gachi_jumpy == 1 and countee == 10 and GetZoneID() ~= 129 then --we only doing jumps if we configured for it
		--if GetZoneID() == 341 then --only need to jump in goblet for now
			yield("/gaction jump")
			countee = 0
			yield("/echo we are still pathfinding apparently")
		end
	end
end

function ZoneTransition()
	iswehehe = IsPlayerAvailable() 
	iswoah = 0
    repeat 
        yield("/wait 0.5")
        yield("/echo Are we ready? -> "..iswoah.."/20")
		iswehehe = IsPlayerAvailable() 
		iswoah = iswoah + 1
		if 	iswoah == 20 then
			iswehehe = false
		end
    until not iswehehe
	iswoah = 0
    repeat 
        yield("/wait 0.5")
        yield("/echo Are we ready? (backup check)-> "..iswoah.."/20")
		iswehehe = IsPlayerAvailable() 
		iswoah = iswoah + 1
		if 	iswoah == 20 then
			iswehehe = true
		end
    until iswehehe
end

function WalkToGC()
    if GetPlayerGC() == 1 then --toilet
        if GetZoneID() ~= 128 then
			yield("/li The Aftcastle") 
		    ZoneTransition()
			if movementtype == 1 then --visland hackery
				yield("/visland exectemponce H4sIAAAAAAAACu2Wy07DMBBF/2XWkRU/Y2eHClQVaikFqTzEwlBXtZTEpXFAqOq/44ZE9LFDWSYrz53J6PrIGnsLE50bSGE4yGxeaohguHHVOigTV5gQzvX32tnCl5C+bGHqSuutKyDdwiOkWCEuZcIieIKUxSiO4BnShKCEYUV3IQpNRpeQhsRML2wVutB91dh9mtwUvs5MtV8tbbGAdKmz0kQwKrzZ6Hc/t3512/x+qDWeg7ty5b7aTLBVnrWoveIIrnLnWycjb/JmeVFXNMFdZUp/uL43H3Uwdm+NfO/deuCKRQMhKDc2ywauarYyc5U3x/bm2vo/X/vo2m2Oe+zFB5ubcaiLd9EZZsKRiqmK5QlniiRjuOfcFWeaIBnz5IwypxwT0R/njjAzgaSQmIsaNFVIhY+3sBnBVLAedkewhUAsID2b0JSJfnJ0NjkSjkigfHoRUkQEUbg/zB1hVhRRoqTgLWeScJr8subh0cFi2bP+F+vX3Q9/zfhXCwoAAA==")
				visland_stop_moving()
			end
		end
		if movementtype == 0 then --default navmesh
			WalkTo(94, 40.5, 74.5)
		end
    elseif GetPlayerGC() == 2 then --vampire coven
		if movementtype == 1 then --visland hackery
			yield("/li Archers' Guild") 
		    ZoneTransition()
			yield("/visland exectemponce H4sIAAAAAAAACu2ZTW/bMAyG/4vOKUGJokT5NnRbUQztunZA94EdssVdDTRxlzgbhqL/ffRXP7CdBh11M2VHoJ+8eE1Sd+Z0ua5NZY4Ov2+blVmYo227v9WF03ZTa3i5/H3bNptuZ6rPd+as3TVd025MdWc+mMp6gRREFuajqQ4QKDCLtbwwn0wlBIFiTPca6V7HL02FC3O+XDV73YxAg5P2Z72uN91w52zZXV81m5WprpY3u3phjjddvV1+6y6b7vrt9POna1PmmuTuuv0139Hsdn9tMaRsF+bVuu3mTI67ej1dvhiemIJ3+3rXPb2+qH8MwUn7dVq+6Nrbw3azmljoypvm5uaw3U+vct7uu/p5epfLpnvMq49et9vne/SL75t1faLP4f3ib9pOIEQf3IwbIwZFHgfcMQFah6HgzoXbekAXgwsPvMX7INGNvB1EywV3PtzqH8k6P9BW2APl4IEYrRRV58KMEUiCpQmzw6Q2IgNsFnD6DxRR56Wtvj1atgrcpuBp/D6yatwic5F2JtiJgaI4HB1E7ZkT+TB+HclDQC/FR3LBFobgUhpZE3hkN5iKstYyJXr2vig7F+zAkKLEgbWFIIgcRssmBFW5wi8ukok1B3Ae/ezYQekqe5ppE1GKRdm5aHsPTCh9DzN4NvuUAobRRyxof4MFdjbYDgQTyT9Y2wQxavfui49kgk064bCB7GTaY9PoSI08BSqUM1FWj3CCkWfKVgvs+MCamC0W2Pm6GW1gtMrrG5hxIpISOnI8WwglH22xkFy8D7zW1T7JzNsBueTF+pE3QwwoEouZ5OLtELRHdNOA1YMldP34r6dNIKrtMs7Op27SPtEFbctn3E766eqkbgtiQ3Kl+svGm0kJE0ea3URsfz4z6RsholYmZaCdjXewYJmJJn1rp44auL6P788PgEPSqLh3Nt4RtLl5LE703IBJHWbAfSDq3+hj0XdO3iye7RPegj7wzNt6F32pTv6P95f7P8KvqWabHwAA")
			visland_stop_moving()
		end
		if movementtype == 0 then --default navmesh
			WalkTo(-68.5, -0.5, -8.5)
		end
    elseif GetPlayerGC() == 3 then --best place in game
		if movementtype == 1 then --visland hackery
			yield("/li Thaumaturges' Guild") 
		    ZoneTransition()
			yield("/visland exectemponce H4sIAAAAAAAACu2WSWvDMBCF/8ucXWFJI8v2raQLoaQ7pAs9uI1CBLGVxnJLCfnvnTgKTZdT8dE+6b2Rh6cPMWgF50VpIIfTQTOfQASnS9csSJ+7ypAcFx8LZytfQ/64gktXW29dBfkK7iA/4EoxybnECO4h58jiWEXwADkmLFGayzUpajQ8gjyO4LqY2IY6SUZi5N5MaSrfVi4LP5vaagL5tJjXJoJh5c2yePFj62cX4fd9L6SmhPXMve8qFK3+1aLNyyM4Lp3fJRl6U4blYbsjiKvG1H5/fWNeWzFyz8G+8W4xcNUkgCDnzM7nA9eEo1y7xpvv8caF9V+5NurELb/32Ji3tjQj2hevoz9Qo2KxEkptUYsNQgLNU4YpatWT7pB0xhQK/EH6gCdMaMGl7i91Z6hlxhKOQv9ALemuo85S1aPuDLVARoOaJnbLGneoNdmJFog96u5QayZRpRnuUGf0od4CzwRLBZWTHnh3E1swjsQ08OYEPNUyjO1YsSxBepn0wP8F/Gn9CUAVPgMlCgAA")
			visland_stop_moving()
		end
		if movementtype == 0 then --default navmesh
			WalkTo(-142.5, 4, -106.5)
		end
    end
end

function TargetedInteract(target)
    yield("/target "..target.." <wait.0.5>")
    yield("/pinteract <wait.1>")
end

function DidWeLoadcorrectly()
	yield("/echo We loaded the functions file successfully!")
end

function CharacterSafeWait()
     yield("/echo 15 second wait for char swap")
	 yield("/wait 15")
	 yield("/waitaddon NamePlate <maxwait.600> <wait.5>")
	 --ZoneTransition()
end

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
 --yield("/echo movement stopped - time for GC turn ins or whatever")
 yield("/echo movement stopped safely - script proceeding to next bit")
 yield("/visland stop")
 yield("/vnavmesh stop")
 yield("/wait 3")
 --added becuase simpletweaks is slow to update :(
 yield("/character")
 yield("/wait 1")
 yield("/pcall Character true 12")
 yield("/wait 1")
 yield("/pcall RecommendEquip true 0")
 yield("/wait 1")
end


function return_to_limsa_bell()
	yield("/tp Limsa Lominsa")
	ZoneTransition()
	yield("/wait 2")
	yield("/wait 1")
	yield("/pcall SelectYesno true 0")
	PathfindAndMoveTo(-125.440284729, 18.0, 21.004405975342, false)
	visland_stop_moving() --added so we don't accidentally end before we get to the inn person
end

function return_to_inn()
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

function return_to_fc()
	--yield("/tp Estate Hall") --old way
	yield("/tp Estate Hall (Free Company)") --new way notice the brackets
	yield("/wait 1")
	--yield("/waitaddon Nowloading <maxwait.15>")
	yield("/wait 15")
	yield("/waitaddon NamePlate <maxwait.600><wait.5>")
end

function return_to_lair()
	--yield("/tp Estate Hall")
	yield("/tp Estate Hall (Private)")
	yield("/wait 1")
	--yield("/waitaddon Nowloading <maxwait.15>")
	yield("/wait 15")
	yield("/waitaddon NamePlate <maxwait.600><wait.5>")
end

function return_fc_entrance()
	yield("/hold W <wait.1.0>")
	yield("/release W")
	yield("/target Entrance <wait.1>")
	yield("/lockon on")
	yield("/automove on <wait.2.5>")
	yield("/interact")
	yield("/automove off <wait.1.5>")
	yield("/hold Q <wait.1.0>")
	yield("/interact")
	yield("/release Q")
	yield("/interact")
	yield("/hold Q <wait.1.0>")
	yield("/interact")
	yield("/release Q")
	yield("/interact")
	yield("/wait 1")
end

function return_fc_near_bell()
	yield("/target \"Summoning Bell\"")
	yield("/wait 2")
	--PathfindAndMoveTo(GetObjectRawXPos("Summoning Bell"), GetObjectRawYPos("Summoning Bell"), GetObjectRawZPos("Summoning Bell"), false)
	WalkTo(GetObjectRawXPos("Summoning Bell"), GetObjectRawYPos("Summoning Bell"), GetObjectRawZPos("Summoning Bell"))
	visland_stop_moving() --added so we don't accidentally end before we get to the bell
end

function are_we_dol()
	is_it_dol = false
	yield("Our job is "..GetClassJobId())
	if type(GetClassJobId()) == "number" and GetClassJobId() > 7 and GetClassJobId() < 19 then
		is_it_dol = true
		yield("/echo We are a Disciple of the (H/L)and")
	end
	return is_it_dol
end

function which_cj()
	highest_cj = 0
	highest_cj_level = 0
	yield("Time to figure out which job ID to switch to !")
	for i=0,7 do
		if tonumber(GetLevel(i)) > highest_cj_level then
			highest_cj_level = GetLevel(i)
			highest_cj = i
			yield("/echo Oh my maybe job->"..i.." lv->"..highest_cj_level.." is the highest one?")
		end
	end
	for i=19,29 do
		if tonumber(GetLevel(i)) > highest_cj_level then
			highest_cj_level = GetLevel(i)
			highest_cj = i
			yield("/echo Oh my maybe job->"..i.." lv->"..highest_cj_level.." is the highest one?")
		end
	end
	return tonumber(highest_cj)
end

function job_short(which_cj)
	yield("Time to figure out which job shortname to switch to !")
	if which_cj == -1 then shortjob = "adv" end
	if which_cj == 1 then shortjob = "gld" 
		if GetItemCount(4542) > 0 then
			shortjob = "pld"
		end
	end
	if which_cj == 0 then shortjob = "pgl" 
		if GetItemCount(4543) > 0 then
			shortjob = "mnk"
		end
	end
	if which_cj == 2 then shortjob = "mrd" 
		if GetItemCount(4544) > 0 then
			shortjob = "war"
		end
	end
	if which_cj == 4 then shortjob = "lnc" 
		if GetItemCount(4545) > 0 then
			shortjob = "drg"
		end
	end
	if which_cj == 3 then shortjob = "arc" 
		if GetItemCount(4546) > 0 then
			shortjob = "brd"
		end
	end
	if which_cj == 6 then shortjob = "cnj" 
		if GetItemCount(4547) > 0 then
			shortjob = "whm"
		end
	end
	if which_cj == 5 then shortjob = "thm" 
		if GetItemCount(4548) > 0 then
			shortjob = "blm"
		end
	end
	if which_cj == 19 then shortjob = "rog" 
		if GetItemCount(7886) > 0 then
			shortjob = "nin"
		end
	end
	if which_cj == 20 then shortjob = "mch" end
	if which_cj == 21 then shortjob = "drk" end
	if which_cj == 22 then shortjob = "ast" end
	if which_cj == 23 then shortjob = "sam" end
	if which_cj == 24 then shortjob = "rdm" end
	if which_cj == 25 then shortjob = "blu" end
	if which_cj == 26 then shortjob = "gnb" end
	if which_cj == 27 then shortjob = "dnc" end
	if which_cj == 28 then shortjob = "rpr" end
	if which_cj == 29 then shortjob = "sge" end
	return shortjob
end

function try_to_buy_fuel(restock_amt)
	--enter house
	yield("/wait 0.5")
	yield("/interact")
	yield("/wait 5")
	--enter workshop
	yield("/target \"Entrance to Additional Chambers\"")
	yield("/wait 0.5")
	yield("/lockon")
	yield("/automove")
	visland_stop_moving()
	yield("/interact")
	yield("/wait 1")
	yield("/pcall SelectString true 0")
	yield("/wait 5")
	--target mammet
	yield("/target mammet")
	yield("/wait 0.5")
	yield("/lockon")
	yield("/automove")
	visland_stop_moving()
	--open mammet menu
	yield("/automove off")
	yield("/interact")
	yield("/wait 2")
	yield("/pcall SelectIconString true 0")
	yield("/wait 2")
	--buy exactly restock_amt final value for fuel
	--grab current fuel total
	curFuel = GetItemCount(10155)
	oldFuel = curFuel + 1
	while curFuel < restock_amt do
		buyamt = 99 --this can be set to 231u if you want but i wouldn't recommend it as it shows on lodestone
		if (restock_amt - curFuel) < 99 then
			buyamt = restock_amt - curFuel
		end
		yield("/pcall FreeCompanyCreditShop false 0 0u "..buyamt.."u") 
		yield("/wait 1")
		oldFuel = curFuel
		curFuel = GetItemCount(10155)
		if oldFuel == curFuel then
			curFuel = restock_amt
			yield("/echo we ran out of FC points before finishing our purchases :(")
		end
	end
	yield("/echo We now have "..GetItemCount(10155).." Ceruelum Fuel Tanks")
	ungabunga()
end