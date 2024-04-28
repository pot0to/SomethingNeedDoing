--[[

      ***********************************************
      *         Triple Triad + Auto Retainer        * 
      *     Play TT and process AR when needed      *
      ***********************************************

      *************************
      *    Author: T0mball    *
      *************************

      **********************
      * Version  |  1.0.1  *
      **********************

      -> 1.0.1  : Fixed issue with summoning bell interaction. Minor changes
      -> 1.0.0  : Basic Functionality, it works. Prob needs a revision

      ***************
      * Description *
      ***************

      This script will tp to Foundation, go to House Fortemps and play TT with the servant. It stops playing when AR needs to be processed and
      goes to the Summoning Bell in Jeweled Crozier. Processes AR and goes back to playing.
      
      Many thanks to Ice and UcanPatatess, a lot of my code is based on theirs.

      *********************
      *  Required Plugins *
      *********************


      Plugins that are used are:
      -> Saucy : https://love.puni.sh/ment.json
      -> Teleporter
      -> Lifestream : https://github.com/NightmareXIV/Lifestream/blob/main/Lifestream/Lifestream.json
      -> Something Need Doing [Expanded Edition] : https://puni.sh/api/repository/croizat
      -> YesAlready : https://love.puni.sh/ment.json
        -> For each of the following categories add the given inputs:
            -> YesNo
                -> "Enter Fortemps Manor?"
                -> "Leave Fortemps Manor?"
            -> Talk
                -> "House Fortemps Guard"
                -> "House Fortemps Manservant"
            -> List (Target Restricted = YES)
                -> text: "Triple Triad Challenge" target: "House Fortemps Manservant"
]]

--[[

  **************
  *  Settings  *
  **************
  ]]
  
--   No Settings Yet :(

--[[

  ************
  *  Script  *
  *   Start  *
  ************

]]

::Functions::

    function ZoneTransition()
        repeat 
            yield("/wait 0.5")
        until not IsPlayerAvailable()
        repeat 
            yield("/wait 0.5")
        until IsPlayerAvailable()
    end

    function TeleportTest()
        while GetCharacterCondition(27) do 
            yield("/wait 1") 
        end
        yield("/wait 1")
        while GetCharacterCondition(45) or GetCharacterCondition(51) do 
            yield("/wait 3") 
        end
    end

    function AetheryteTeleport()
        while GetCharacterCondition(32) do
            yield("/wait 1")
        end
        yield("/wait 1")
        while GetCharacterCondition(45) or GetCharacterCondition(51) do
            yield("/wait 1") 
        end
    end

    function DistanceToVendor()
        if IsInZone(478) then -- Idyllshire
            Distance_Test = GetDistanceToPoint(-19.277, 211, -36.076)
        end
    end

    function PathFinding()
        yield("/wait 0.2")
        while PathfindInProgress() do
            yield("/wait 0.5")
        end
    end
    
    function moveToTarget(minDistanceOverride)
        minDistance = minDistanceOverride or 7
        targetX = GetTargetRawXPos()
        targetY = GetTargetRawYPos()
        targetZ = GetTargetRawZPos()
        PathfindAndMoveTo(targetX, targetY, targetZ, false)
        PathFinding()
        while GetDistanceToPoint(targetX, targetY, targetZ) > minDistance do 
            yield("/wait 0.1")
        end 
        PathStop()
    end

::TPFoundation::

    while IsInZone(418) == false and GetCharacterCondition(27) == false do
        yield("/tp Foundation")
        yield("/wait 1")
    end

    TeleportTest()

    if IsInZone(418) == false and GetCharacterCondition(27) == false then
        yield("/echo Hmm.... either you moved, or the teleport failed, lets try that again")
        yield("/wait 0.5")
        goto TPFoundation
    end

    if IsInZone(418) then -- Foundation
        --LogInfo("[TT+AR] Currently in Foundation!")
        yield("/wait 3")
        yield("/target Aetheryte")
        moveToTarget()
        yield("/li Last")
        --LogInfo("[TT+AR] Heading to The Last Vigil")
    end

::HouseFortemps::
    ZoneTransition()
    yield("/wait 1")
    yield("/target House Fortemps Guard")
    moveToTarget()
    yield("/interact")
    ZoneTransition()
    yield("/target House Fortemps Manservant")
    moveToTarget()
    yield("/interact")
    yield("/wait 1")
    yield("/saucy tt go")
    yield("/wait 1")
    
    while ARAnyWaitingToBeProcessed() == false do
        yield("/wait 1")
    end
    
    -- AR is ready, play one last game
    yield("/saucy tt play 1")
    
    -- Wait for last game to finish
    while GetCharacterCondition(13)==true do
        yield("/wait 1")
    end
    
    -- Go to bell
    yield("/wait 1")
    yield("/e Done Playing... Heading to bell")
    
    yield("/target Manor Exit")
    moveToTarget(4)
    yield("/interact")
    ZoneTransition()
    
    yield("/target Aethernet Shard")
    moveToTarget()
    yield("/li Jeweled")
    ZoneTransition()
    
    yield("/target Summoning Bell")
    moveToTarget(4)
    
    if ARAnyWaitingToBeProcessed() == true then
        yield("/interact")
        yield("/wait 1")
        while ARAnyWaitingToBeProcessed() == true and IsPlayerOccupied() == true do
            -- yield("/e Processing Retainers... Player is occupied")
            yield("/wait 1")
        end
        yield("/waitaddon RetainerList")
        yield("/e Finished processing retainers...")
        yield("/pcall RetainerList true -1")
        yield("/wait 1")
    end
    
    -- Continue playing TT
    yield("/target Aethernet Shard")
    moveToTarget()
    yield("/li Last")
    -- ZoneTransition()
    goto HouseFortemps

::StoppingScript::
    yield("/echo Stopping script, thanks for using")
