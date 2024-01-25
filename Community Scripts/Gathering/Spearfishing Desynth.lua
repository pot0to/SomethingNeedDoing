--[[
  Description: Spearfishing Auto Desynth
  The script allows you to have it running a visland route (while spearfishing) and when you get to a certain inventory amount it will pause for you and proceed to desynth all your collectables.
  Version: 5 (Now with a built in route!)
  Author: LegendofIceman
]]

--[[Route name below, it's in base64 (which in simple terms, means that the route is already in this LUA script 
    This makes it to where you won't need to import the script to visland, and run it solely from this
    Make sure to have the loop button pressed in visland!]]
routename = "H4sIAAAAAAAACu2VTU/cMBCG/wryOYz8Mf7KDRWQ9kBbqkpbinpwWZeN2sRVYkBotf+94yRbVIHUAycWbh5nMhk/ft/Jhr0PbWQ1Owl9Xn/vY/h5cBTzOvZD6FasYstw/zs1XR5YfblhH9PQ5CZ1rN6wL6yWyoHXwlXsgtWH2oBTqGzFvlJkLKBGVFsKUxcXx6zmFfsUVs0N1ZJAwVm6jW3sMqtFxRZdjn24yssmrz+UbIXI+b/7c685xrbprg/uAj0aqMlhne52edQd1f8Rfg3x4eWxZfrISZvyrpVFju28PBoz5uD8Jg55XpfCy9Dkh4olOk39u9StZhJ82vzctPGM8vi2esRJaA+I1okJlHFglRVWj6SMLJFD8zQp9X9ST1N6CVycKGdXExarQWhtJ/3YIibuxLPkI/dEPkqCcUUuIyYFxihuRkyapOTR4hum4jIBSpidyUgjikvpRk4oaFBxLl6fx7wHQxbDvx5Tzkg5zWiNILi19lnqEfthMq9AojE7TgocyqKlwslzMCike+NENyQ80ECSNH9GmyFoZSRhK6CsB+0d3/tf2bftH0ShbN45CQAA"

--How many slots do you want open before it starts to desynth?
--Default is 5
slots_remaining = 5

-- Starts the route/resumes it if you had it paused in visland
::Fishingstart::
  yield("/visland exectemp "..routename)
  yield("/visland resume")
  yield("/wait 1")

-- Checks to see how much inventory space you have
::StartCount::
i_count = tonumber(GetInventoryFreeSlotCount())

-- Does a constant check to see if the current inventory amount is less than the amount of slots you wanted open
::InventoryTest::
while not (i_count <= slots_remaining) do
  --yield("/echo Slots Remaining: "..i_count) -- Delete the -- in front if you want it to tell you how many slots are left
  yield("/wait 1")
  i_count = tonumber(GetInventoryFreeSlotCount())
    if GetCharacterCondition(6) then
      if HasStatusId(2778, 0) then
      yield("/ac \"Thaliak's Favor\"")
	  end
	end	
end
yield("/echo Inventory has reached "..i_count)
yield("/echo Time to Desynth")

-- test to see if you're still in the spearfishing menu
::GatherTest::
if GetCharacterCondition(6) then
  yield("/visland pause")
  yield("/wait 5")
  goto GatherTest
end

-- Once out of spearfishing menu, dismounts and starts the process for desynthing the fish
::DesynthStart::
if not GetCharacterCondition(6) then
  yield("/wait 1")
  yield("/visland pause")
  yield("/wait 1")
    if GetCharacterCondition(4) then
      yield("/ac dismount")
      end
  yield("/wait 3")
  yield('/ac "Aetherial Reduction"')
  yield("/waitaddon PurifyItemSelector")  -- Need to another check for the mount here, just on the off chance it still pops up
    if GetCharacterCondition(4) then
  yield("/ac dismount") 
  yield("/wait 0.2")
  yield("/pcall PurifyItemSelector True -2")
  yield("/wait 0.2")
  yield('/ac "Aetherial Reduction"')
      yield("/waitaddon PurifyItemSelector")
  
  end
  yield("/pcall PurifyItemSelector true 12 0")
  yield("/waitaddon PurifyResult")
  yield("/pcall PurifyResult True 0")
  yield("/wait 4")
end

-- checks to see if you're desynth'ing yet, then once it's done, resumes the route/process
::DesynthAll::
if GetCharacterCondition(39) then
  yield("/wait 3")
  goto DesynthAll
end 
if not GetCharacterCondition(39) then
  yield("/pcall PurifyAutoDialog True -2")
  yield("/pcall PurifyItemSelector True -1")
  yield("/visland resume")
end

goto StartCount