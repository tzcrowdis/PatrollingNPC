# PatrollingNPC
This NPC patrols an object of interest, defends the object if it's healthy, and flees if it's health is too low.

## States
### Patrol
The agent walks around the object of interest until an enemy enters the agents FOV. The direction the agent walks is determined by the generatePath.cs script in the Agent Scripts folder. The directions were set manually and point the agent counterclockwise around the object of interest and towards the line $(\frac{1}{4}x)^4 + (\frac{1}{4}z)^4 = 1$ with greater intensity the farther the agent is from that line. The patrolUpdate variable controls how often the agents direction is updated. It updats every 0.6s as this created a meandering-like movement.
### Defend
Upon entering the Defend state after spotting an enemy the agent first finds and crouches behind cover. This cover is on the opposite side of the object of interest from the enemy. Once in cover the agent randomly switches between three substates. In the first state the agent stands up from behind cover and shoots at the enemy. In the second state the agent remains crouched and moves left until it has line of sight to the enemy and starts shooting. The third state is the same as the second except the agent moves right to find the enemy.
### Flee
The agent enters this state when its health is below a certain threshold. It runs directly away from the enemy and at somewhat random intervals switches between straight motion and serpentine. This is done by changing the amplitude of this function:
$$\vec{d} = A Sin(\omega t) \vec{v}_{\perp} + \vec{v}$$
Where $\vec{d}$ is the direction vector of our agent and $\vec{v}$ is the vector from the enemy to the agent. If the agent is still alive after a certain amount of time it will return to patrolling the object of interest.
## Test Scripts
The script agentTest.cs attached to the agent showcases the 3 main states. Recommend starting with no states set to true then using the number keys to select a state at runtime, default is Patrol.

The enemyTestBehavior.cs Script in Test Scripts currently does nothing. In the future it could be set to lerping between two points to simulate movement.
## Demo Video
https://github.com/tzcrowdis/PatrollingNPC/assets/100492586/fedf80ac-9682-4ff6-91bd-61b65700c577
## Future Directions
I think it could be interesting to generate nodes according to $(\frac{1}{4}x)^4 + (\frac{1}{4}z)^4 = 1$, varying the fraction between 0 and 1, then implementing this with A* as the primary method of navigation. Could provide a more general way of finding paths.
## Misc
Unity version: 2021.3.15f1
