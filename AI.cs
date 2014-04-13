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

      // want 1 terminator and hacker per 2 archers and 3 claws
      bool spawnClaws = 3 * terminators > claws;
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
      for (int i = 0; i < mapHeight(); i++)
      {
          // enough scrap
          if (players[playerID()].ScrapAmount >= cost)
          {
              // nothing spawning here
              if (getTile((mapWidth() - 1) * playerID(), i).TurnsUntilAssembled == 0)
              {
                  if (!myUnits.getValueFromSpot((mapWidth() - 1) * playerID(), i))
                  {
                      // spawn it
                      players[playerID()].orbitalDrop((mapWidth() - 1) * playerID(), i, unitID);
                  }
              }
          }
      }

      //CIA.runMissions(Strat.AssignMissions(droids, playerID(), boardState));

      Func<Point, bool> isWalkable = delegate(Point p)
      {
          return boardState.walkable.getValueFromSpot(p.X, p.Y);
      };
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
          return boardState.theirMovables.getValueFromSpot(p.X, p.Y);
      };

      for (int i = 0; i < droids.Length; i++)
      {
          if (droids[i].MovementLeft > 0 && ((droids[i].Owner == playerID() && droids[i].HackedTurnsLeft == 0) || (droids[i].Owner != playerID() && droids[i].HackedTurnsLeft > 0)))
          {
              if (!(droids[i].Variant == (int)Unit.HACKER))
              {
                  CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isEnemyHangar, isWalkable, true));
              }
              else
              {
                  CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isGoalHacker, isWalkable, true));
              }
              boardState.update(droids);
          }
      }

      // ATTACK
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
                      Func<Point, bool> walkable = spot =>
                      {
                          return boardState.walkable.getValueFromSpot(spot.X, spot.Y);
                      };
                      CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], target, walkable, true));
                  }
                  else
                  {
                      Bb targets = new Bb(boardState.ourHangers.width, boardState.ourHangers.height);
                      targets.board = targets.board.Or(boardState.theirHangers.board);
                      targets.board = targets.board.Or(boardState.theirMovables.board);
                      targets.board = targets.board.Or(boardState.theirImmovables.board);
                      Func<Point, bool> hackerTarget = spot =>
                      {
                          return boardState.theirMovables.getValueFromSpot(spot.X, spot.Y);
                      };
                      Func<Point, bool> target = spot =>
                      {
                          return targets.getValueFromSpot(spot.X, spot.Y);
                      };
                      Func<Point, bool> walkable = spot =>
                      {
                          return boardState.walkable.getValueFromSpot(spot.X, spot.Y);
                      };
                      if (droids[i].Variant != (int)Unit.HACKER)
                      {
                          CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], target, walkable, true));
                      }
                      else
                      {
                          CIA.runMission(new Mission(MissionTypes.attackInRange, droids[i], hackerTarget, walkable, true));
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
