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
      Func<Point, bool> isWalkable = delegate(Point p)
      {
          return boardState.walkable.getValueFromSpot(p.X, p.Y);
      };
      Func<Point, bool> isGoal = delegate(Point p)
      {
          return boardState.theirHangers.getValueFromSpot(p.X, p.Y);
      };
      for (int i = 0; i < droids.Length; i++)
      {
          if (droids[i].MovementLeft > 0 && droids[i].Owner == playerID())
          {
              CIA.runMission(new Mission(MissionTypes.goTo, droids[i], isGoal, isWalkable));
              boardState.update(droids);
          }
      }

      Console.WriteLine("Turn Number: " + turnNumber().ToString());
      for (int i = 0; i < mapHeight(); i++)
      {
          //make sure you own enough scrap
          if (players[playerID()].ScrapAmount >= modelVariants[(int)Unit.CLAW].Cost)
          {
              //make sure nothing is spawning there
              if (getTile((mapWidth() - 1) * playerID(), i).TurnsUntilAssembled == 0)
              {
                  //make sure there isn't a hangar there
                  if (!boardState.ourHangers.getValueFromSpot((mapWidth() - 1) * playerID(), i))
                  {
                      //spawn the claw
                      players[playerID()].orbitalDrop((mapWidth() - 1) * playerID(), i, (int)Unit.CLAW);
                  }
              }
          }
      }
      

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
