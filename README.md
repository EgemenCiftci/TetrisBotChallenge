# Tetris Bot Challenge

## Goal
Create a Tetris bot that gets the maximum score in 10 random games.

## Idea
If I create a fixed set of 1000 games and create a bot that will get the maximum average score, it will play well in 10 random games.

## Methodology
1. Simulate every rotation and position​
2. Calculate objective value​
3. Select the rotation and position where the objective value is best​
4. Apply the rotation and position

## Objective Value​
- Aggregate height
- Cleared lines
- Holes
- Bumpiness

## Objective Function
OV = CL×CLW – AH×AHW – H×HW – B×BW

OV: Objective Value​\
CL: Cleared Lines​\
CLW: Cleared Lines Weight​\
AH: Aggregate Height​\
AHW: Aggregate Height Weight​\
H: Holes​\
HW: Holes Weight​\
B: Bumpiness​\
BW: Bumpiness Weight

## Weight Optimization
### 1. One-digit brute-force search​
- Searched between 0 and 9 for each weight​
- 10 × 10 × 10 × 10 = 10000 iterations​
- Played 1000 games​
- The total duration is 11 hours ​
- The best score per game is 1513.333​
- Best parameters are 9 | 9 | 9 | 4​

### 2. Two-digit brute-force search​
- 90 | 90 | 90 | 40 gives same results as 9 | 9 | 9 | 4​
- Searched between -5 and +5 for each weight​
- 10 × 10 × 10 × 10 = 10000 iterations​
- Played 1000 games​
- The total duration was 22 hours ​
- The best score per game is 1589.222​
- Best parameters are 89 | 92 | 89 | 40

### 3. Three-digit brute-force search
- 890 | 920 | 890 | 400 gives same results as 89 | 92 | 89 | 40​
- Searched between -5 and +5 for each weight​
- 10 × 10 × 10 × 10 = 10000 iterations​
- Played 1000 games​
- The total duration was 26 hours ​
- The best score per game is 1589.222​
- There is no improvement in the best score

## Results & Conclusion
- The current average score per game is 1589 for 1,000 games​
- This is a simple solution​
- It can be further optimized using more parameters and different optimization algorithms
