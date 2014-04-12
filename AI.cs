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
  HANGAR = 7,
};

class AI : BaseAI
{
    int spawnX = 4, spawnY = 4;

  public override string username()
  {
    return "Show Me Your Nuts";
  }

  public override string password()
  {
    return "droids";
  }

  public override bool run()
  {
      BoardState boardState = new BoardState(droids, mapWidth(), mapHeight(), playerID());
      Func<Point, bool> isWalkable = delegate(Point p)
      {
          return boardState.walkable.getValueFromSpot(p.X, p.Y);
      };
      Func<Point, bool> isGoal = delegate(Point p)
      {
          return boardState.ourHangers.getValueFromSpot(p.X, p.Y);
      };
      for (int i = 0; i < droids.Length; i++)
      {
          if (droids[i].MovementLeft > 0 && droids[i].Owner == playerID())
          {
              CIA.goTo(new Mission(MissionTypes.goTo, droids[i], isGoal, isWalkable));
              boardState.update(droids);
          }
      }



      //try to spawn a claw near your side
      //make sure you own enough scrap
      Console.WriteLine("Turn Number: " + turnNumber().ToString());
      if (players[playerID()].ScrapAmount >= modelVariants[(int)Unit.CLAW].Cost)
      {
          bool spawning = false;
          while (!spawning)
          {
              //make sure nothing is spawning there
              if (getTile(spawnX, spawnY).TurnsUntilAssembled == 0)
              {
                  bool spawn = true;
                  //make sure there isn't a hangar there
                  for (int i = 0; i < droids.Length; i++)
                  {
                      //if the droid's x and y is the same as the spawn point
                      if (droids[i].X == spawnX && droids[i].Y == spawnY)
                      {
                          //if the droid is a hangar
                          if (droids[i].Variant == (int)Unit.HANGAR)
                          {
                              //can't spawn on top of hangars
                              spawn = false;
                              break;
                          }
                      }
                  }
                  if (spawn)
                  {
                      //spawn the claw
                      spawning = true;
                      players[playerID()].orbitalDrop(spawnX, spawnY, (int)Unit.CLAW);
                  }
              }
              else
              {
                  spawnY++;
                  if (spawnY >= mapHeight())
                      spawnY = 0;
              }
          }
      }
      //loop through all of the droids
     //loop through all of the droids
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
                      Bb targets = new Bb(mapWidth(), mapHeight());
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
                      Mission attack = new Mission(MissionTypes.attackInRange, droids[i], target, walkable);
                      CIA.runMission(attack);
                  }
                  else
                  {
                      Bb targets = new Bb(mapWidth(), mapHeight());
                      targets.board = targets.board.Or(boardState.theirHangers.board);
                      targets.board = targets.board.Or(boardState.theirMovables.board);
                      targets.board = targets.board.Or(boardState.theirImmovables.board);
                      Func<Point, bool> target = spot =>
                      {
                          return targets.getValueFromSpot(spot.X, spot.Y);
                      };
                      Func<Point, bool> walkable = spot =>
                      {
                          return boardState.walkable.getValueFromSpot(spot.X, spot.Y);
                      };
                      Mission attack = new Mission(MissionTypes.attackInRange, droids[i], target, walkable);
                      CIA.runMission(attack);
                  }
              }
          }
      }

      return true;
  }

  public override void init()
  {
      Searcher.mapHeight = mapHeight();
      Searcher.mapWidth = mapWidth();
    int offset = 0;
    bool found = false;
    while(!found)
    {
      //find a location without a hangar
      for(int i = 0; i < tiles.Length; i++)
      {
        //make sure that the tile is near the edge
        if(tiles[i].X == (mapWidth() - 1) * playerID() + offset)
        {
          bool hangarPresent = false;
          //check for hangar
          for(int z = 0; z < droids.Length; z++)
          {
            if(droids[z].X == tiles[i].X && droids[z].Y == tiles[i].Y)
            {
              hangarPresent = true;
              break;
            }
          }
          if(!hangarPresent)
          {
            spawnX = tiles[i].X;
            spawnY = tiles[i].Y;
            found = true;
            break;
          }
        }
      }
      //if nothing was found then move away from the edge
      if(!found)
      {
        //if on the left
        if(playerID() == 0)
        {
          offset++;
        }
        else
        {
          //on the right
          offset--;
        }
      }
    }
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

  public AI(IntPtr c)
      : base(c) { }
}
