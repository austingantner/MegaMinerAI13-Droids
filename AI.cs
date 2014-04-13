using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

enum Unit
{
  CLAW = 0,
  ARCHER = 1,
  REPAIRER = 2,
  HACKER = 3,
  TURRET = 4,
  WALL = 5,
  TERMINATOR = 6,
  HANGAR = 7
};

class AI : BaseAI
{
  public override string username()
  {
    return "Droids Gone Wild: Show Me Your Nuts";
  }

  public override string password()
  {
    return "droids";
  }

  public override bool run()
  {
      BoardState boardState = new BoardState(droids, mapWidth(), mapHeight(), playerID());      

      Console.WriteLine("Turn Number: " + turnNumber().ToString());

      // Find rightmost enemy turret and spawn terminator
      // If this is our first turn
      if (turnNumber() < 2)
      {
          int x = 10;
          int y = 0;
          bool foundSpot = false;
          for (int i = 0; i < droids.Length; i++)
          {
              if (droids[i].Variant == (int)Unit.TURRET)
              {
                  if (droids[i].Owner != playerID())
                  {
                      if (getTile(droids[i].X, droids[i].Y).TurnsUntilAssembled == 0)
                      {
                          foundSpot = true;
                          if (playerID() == 0)
                          {
                              if (droids[i].X > x)
                              {
                                  x = droids[i].X;
                                  y = droids[i].Y;
                              }
                          }
                          else
                          {
                              if (droids[i].X < x)
                              {
                                  x = droids[i].X;
                                  y = droids[i].Y;
                              }
                          }
                      }
                  }
              }
          }
          if (players[playerID()].ScrapAmount >= modelVariants[(int)Unit.TERMINATOR].Cost && foundSpot)
          {
              players[playerID()].orbitalDrop(x, y, (int)Unit.TERMINATOR);
          }
      }

      // Find and spawn claws on enemy turrets
      // also count how many of each type we have
      int terminators = 0, claws = 0, archers = 0, hackers = 0;
      for (int i = 0; i < droids.Length; i++)
      {
          if (droids[i].Variant == (int)Unit.TURRET)
          {
              if (droids[i].Owner != playerID())
              {
                  if (getTile(droids[i].X, droids[i].Y).TurnsUntilAssembled == 0)
                  {
                      if (players[playerID()].ScrapAmount >= modelVariants[(int)Unit.CLAW].Cost)
                      {
                          players[playerID()].orbitalDrop(droids[i].X, droids[i].Y, (int)Unit.CLAW);
                      }
                  }
              }
          }
          else
          {
              if (droids[i].Owner == playerID())
              {
                  switch (droids[i].Variant)
                  {
                      case (int)Unit.TERMINATOR:
                          terminators++;
                          break;
                      case (int)Unit.CLAW:
                          claws++;
                          break;
                      case (int)Unit.ARCHER:
                          archers++;
                          break;
                      case (int)Unit.HACKER:
                          hackers++;
                          break;
                  }
              }
          }
      }

      // DETECT WALLS
      int numUnits = 0;
      int colWithWall = -1;
      for (int i = 15; i < 25; i++)
      {
          int tempCount = 0;
          for (int j = 0; j < 20; j++)
          {
              if (boardState.theirMovables.getValueFromSpot(i, j) && !(getTile(i, j).TurnsUntilAssembled > 0))
                  tempCount++;
          }
          if (tempCount > numUnits)
          {
              numUnits = tempCount;
              colWithWall = i;
          }
      }
      // SPAWN CLAWS ON WALLS
      if (numUnits > 5)
      {
          for (int i = 0; i < 20; i++)
          {
              if (getTile(colWithWall, i).TurnsUntilAssembled == 0 && boardState.theirMovables.getValueFromSpot(colWithWall, i))
              {
                  if (players[playerID()].ScrapAmount >= modelVariants[(int)Unit.CLAW].Cost)
                  {
                      players[playerID()].orbitalDrop(colWithWall, i, (int)Unit.CLAW);
                  }
              }
          }
      }

      Func<Point, bool> isWalkable = delegate(Point p)
      {
          return boardState.walkable.getValueFromSpot(p.X, p.Y);
      };

      // Search for best spawn location
      int bestRow = 0;
      Bb middleRows = new Bb(mapWidth(), mapHeight());
      HashSet<int> badRows = new HashSet<int>();
      for (int i = 19; i < 21; i++)
      {
          for (int j = 0; j < mapHeight(); j++)
          {
              middleRows.setValueAtSpot(i, j);
          }
      }
      bool foundRow = false;
      int curCol = (playerID() == 0) ? 0 : mapWidth() - 1;
      Func<Point, bool> middleTarget = spot =>
      {
          return middleRows.getValueFromSpot(spot.X, spot.Y);
      };
      Bb spawnWalkable = new Bb(mapWidth(), mapHeight());
      spawnWalkable.board = spawnWalkable.board.Or(boardState.walkable.board);
      spawnWalkable.board = spawnWalkable.board.Or(boardState.ourMovables.board);
      spawnWalkable.board = spawnWalkable.board.Or(boardState.theirMovables.board);
      int searches = 0;
      Func<Point, bool> spawnWalkableFunc = spot =>
      {
          return spawnWalkable.getValueFromSpot(spot.X, spot.Y);
      };
      while (!foundRow && searches < 5)
      {
          int shortest = 50;
          searches++;
          for (int i = 0; i < mapHeight(); i++)
          {
              IEnumerable<Point> path = Searcher.findPath(new Point(curCol, i), middleTarget, spawnWalkableFunc);
              int temp = -1;
              foreach (Point p in path)
              {
                  temp++;
              }
              if (temp != -1 && temp < shortest)
              {
                  shortest = temp;
                  bestRow = i;
                  foundRow = true;
              }
              else if (temp == -1)
              {
                  badRows.Add(i);
              }
          }
          if (!foundRow)
          {
              curCol = playerID() == 0 ? curCol + 1 : curCol - 1;
              badRows.Clear();
              if (curCol < 0 || curCol >= mapWidth())
              {
                  curCol = (playerID() == 0) ? 0 : mapWidth() - 1;
                  break;
              }
          }
      }
      if (searches >= 5)
      {
          curCol = playerID() == 0 ? curCol + 1 : curCol - 1;
          badRows.Clear();
          bestRow = 0;
      }
      // want 1 terminator and hacker per 2 archers and 3 claws
      bool spawnClaws = terminators > claws && turnNumber() < 350;
      bool spawnArch = 2 * terminators > archers && !spawnClaws;
      bool spawnHack = terminators > hackers && turnNumber() > 100 && !spawnArch;

      int cost = 10;
      int unitID = 0;
      if (spawnClaws)
      {
          unitID = (int)Unit.CLAW;
          cost = modelVariants[(int)Unit.CLAW].Cost;
      }
      else if (spawnArch)
      {
          unitID = (int)Unit.ARCHER;
          cost = modelVariants[(int)Unit.ARCHER].Cost;
      }
      else if (spawnHack)
      {
          unitID = (int)Unit.HACKER;
          cost = modelVariants[(int)Unit.HACKER].Cost;
      }
      else
      {
          unitID = (int)Unit.TERMINATOR;
          cost = modelVariants[(int)Unit.TERMINATOR].Cost;
      }

      Bb myUnits = new Bb(mapWidth(), mapHeight());
      myUnits.board = myUnits.board.Or(boardState.ourHangers.board);
      myUnits.board = myUnits.board.Or(boardState.ourImmovables.board);
      myUnits.board = myUnits.board.Or(boardState.ourMovables.board);
      int iter = bestRow;
      int rowsChecked = 0;
      while(rowsChecked < mapHeight())
      {
          // Not a bad row
          if (!badRows.Contains(iter))
          {
              // enough scrap
              if (players[playerID()].ScrapAmount >= cost)
              {
                  // nothing spawning here
                  if (getTile(curCol, iter).TurnsUntilAssembled == 0)
                  {
                      if (!myUnits.getValueFromSpot(curCol, iter))
                      {
                          // spawn it
                          players[playerID()].orbitalDrop(curCol, iter, unitID);
                      }
                  }
              }
              iter++;
              if (iter >= mapHeight())
                  iter = 0;
          }
          rowsChecked++;
      }


      // ATTACK
      Func<Point, bool> isAttackable = delegate(Point p)
      {
          return boardState.theirHangers.getValueFromSpot(p.X, p.Y) || boardState.hackTargets.getValueFromSpot(p.X, p.Y);
      };
      Func<Point, bool> nope = delegate(Point p)
      {
          return false;
      };
      for (int i = 0; i < droids.Length; i++)
      {
          //if you have control of the droid
          if ((droids[i].Owner == playerID() && droids[i].HackedTurnsLeft <= 0) ||
              (droids[i].Owner != playerID() && droids[i].HackedTurnsLeft > 0))
          {
              //if there are any attacks left
              if (droids[i].AttacksLeft > 0)
              {
                  if (droids[i].Variant == (int)Unit.REPAIRER)
                  {
                      Bb targets = new Bb(boardState.ourHangers.width, boardState.ourHangers.height);
                      targets.board = targets.board.Or(boardState.ourHangers.board);
                      targets.board = targets.board.Or(boardState.ourMovables.board);
                      targets.board = targets.board.Or(boardState.ourImmovables.board);
                      Func<Point, bool> target = spot =>
                      {
                          return targets.getValueFromSpot(spot.X, spot.Y);
                      };
                      CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], target, isWalkable, nope));
                  }
                  else
                  {
                      Func<Point, bool> hackerTarget = spot =>
                      {
                          return boardState.hackTargets.getValueFromSpot(spot.X, spot.Y);
                      };
                      Func<Point, bool> target = spot =>
                      {
                          return boardState.attackTargets.getValueFromSpot(spot.X, spot.Y);
                      };

                      if (droids[i].Variant != (int)Unit.HACKER)
                      {
                          CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], target, isWalkable, nope));
                      }
                      else
                      {
                          CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], hackerTarget, isWalkable, nope));
                      }
                  }
                  boardState.update(droids);
              }
          }
      }
      
      Func<Point, bool> isEnemyHangar = delegate(Point p)
      {
          return boardState.theirHangers.getValueFromSpot(p.X, p.Y);
      };
      Func<Point, bool> isNotAttacked = delegate(Point p)
      {
          return boardState.notAttackedByEnemy.getValueFromSpot(p.X, p.Y);
      };
      Func<Point, bool> isGoalHacker = delegate(Point p)
      {
          return boardState.hackTargets.getValueFromSpot(p.X, p.Y);
      };

      for (int i = 0; i < droids.Length; i++)
      {
          if (droids[i].MovementLeft > 0 && ((droids[i].Owner == playerID() && droids[i].HackedTurnsLeft == 0) || (droids[i].Owner != playerID() && droids[i].HackedTurnsLeft > 0)))
          {
              if (droids[i].HealthLeft < .3 * droids[i].MaxHealth)
              {
                  Bb spotsOnOurSide = new Bb(mapWidth(), mapHeight());
                  spotsOnOurSide.board = spotsOnOurSide.board.Or(boardState.ourHalf.board);
                  Bb theirUnitsOurSide = new Bb(mapWidth(), mapHeight());
                  theirUnitsOurSide.board = theirUnitsOurSide.board.Or(boardState.ourHalf.board);
                  theirUnitsOurSide.board = theirUnitsOurSide.board.And(boardState.theirMovables.board);
                  Func<Point, bool> attackOurSide = spot =>
                  {
                      return theirUnitsOurSide.getValueFromSpot(spot.X, spot.Y);
                  };
                  CIA.runMission(new Mission(MissionTypes.goTo, droids[i], attackOurSide, isWalkable, isAttackable));
                  spotsOnOurSide.board = spotsOnOurSide.board.And(boardState.walkable.board);
                  Func<Point, bool> runAway = spot =>
                  {
                      return spotsOnOurSide.getValueFromSpot(spot.X, spot.Y);
                  };
                  CIA.runMission(new Mission(MissionTypes.goTo, droids[i], runAway, isWalkable, isAttackable));
              }
              else
              {
                  if (!(droids[i].Variant == (int)Unit.HACKER))
                  {
                      if (droids[i].Variant == (int)Unit.TERMINATOR)
                      {
                          if (!CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isEnemyHangar, isWalkable, isAttackable)))
                          {
                              CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isGoalHacker, isWalkable, isAttackable));
                          }
                      }
                      else
                      {
                          if (!CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isGoalHacker, isWalkable, isAttackable)))
                          {
                              CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isEnemyHangar, isWalkable, isAttackable));
                          }
                      }
                  }
                  else
                  {
                      CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isGoalHacker, isWalkable, isAttackable));
                  }
              }
              boardState.update(droids);
          }
      }

      // ATTACK again
      for (int i = 0; i < droids.Length; i++)
      {
          //if you have control of the droid
          if ((droids[i].Owner == playerID() && droids[i].HackedTurnsLeft <= 0) ||
              (droids[i].Owner != playerID() && droids[i].HackedTurnsLeft > 0))
          {
              //if there are any attacks left
              if (droids[i].AttacksLeft > 0)
              {
                  if (droids[i].Variant == (int)Unit.REPAIRER)
                  {
                      Bb targets = new Bb(boardState.ourHangers.width, boardState.ourHangers.height);
                      targets.board = targets.board.Or(boardState.ourHangers.board);
                      targets.board = targets.board.Or(boardState.ourMovables.board);
                      targets.board = targets.board.Or(boardState.ourImmovables.board);
                      Func<Point, bool> target = spot =>
                      {
                          return targets.getValueFromSpot(spot.X, spot.Y);
                      };
                      CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], target, isWalkable, nope));
                  }
                  else
                  {
                      Func<Point, bool> hackerTarget = spot =>
                      {
                          return boardState.hackTargets.getValueFromSpot(spot.X, spot.Y);
                      };
                      Func<Point, bool> target = spot =>
                      {
                          return boardState.attackTargets.getValueFromSpot(spot.X, spot.Y);
                      };
                      if (droids[i].Variant != (int)Unit.HACKER)
                      {
                          CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], target, isWalkable, nope));
                      }
                      else
                      {
                          CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], hackerTarget, isWalkable, nope));
                      }
                  }
                  boardState.update(droids);
              }
          }
      }

      #region Old Spawn Code
      //for (int i = 0; i < mapHeight(); i++)
      //{
      //    //make sure you own enough scrap
      //    if (players[playerID()].ScrapAmount >= modelVariants[(int)Unit.CLAW].Cost)
      //    {
      //        //make sure nothing is spawning there
      //        if (getTile((mapWidth() - 1) * playerID(), i).TurnsUntilAssembled == 0)
      //        {
      //            //make sure there isn't a hangar there
      //            if (!boardState.ourHangers.getValueFromSpot((mapWidth() - 1) * playerID(), i))
      //            {
      //                //spawn the claw
      //                players[playerID()].orbitalDrop((mapWidth() - 1) * playerID(), i, (int)Unit.CLAW);
      //            }
      //        }
      //    }
      //}
      #endregion

      

      return true;
  }

  public override void init()
  {
    Searcher.mapHeight = mapHeight();
    Searcher.mapWidth = mapWidth();
  }

  /// <summary>
  /// This function is called once, after your last turn.
  /// </summary>
  public override void end() { }

//This functions returns a pointer to a tile, or returns null for an invalid tile
Tile getTile(int x, int y)
{
  if(x >= mapWidth() || x < 0 || y >= mapHeight() || y < 0)
  {
    return null;
  }
  return tiles[y + x * mapHeight()];
}

  public AI(IntPtr c) : base(c) { }
}
