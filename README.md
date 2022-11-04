# VirtualDrawing

## Introduction
This project will cover how to heighten the drawing experience in Virtual Reality. To this end, we will be looking at various scenarios, which will augment the drawing actions the user inputs. As the user draws characters or text, the virtual surface will move accordingly. That way the user will not have to change his hand positioning, which should be benefitial for the virtual experience. Additionally we will implement a scalable surface, which will augment the users input by scaling it according to a factor. 

## Scenario I Non-augmented Text
To find out if our text augmentation leads to a better drawing experience, we first need to determine what the experience without said augmentation shows. The scenario includes a drawable surface on top of a table. The user is not limited in what text or pictures he is supposed to draw (*subject to change*).

## Scenario II Moving virtual surface
The first experiment to augment the users input can be summarized by the graphic below.

![image](https://user-images.githubusercontent.com/116259509/199894705-feaf73ac-c501-4a83-a8b4-af317d108276.png)

In this case the user keeps inputting in the same physical location, as the virtual surface moves in a predetermined direction. The speed in which it moves needs to be calculated by the speed of input of the user. To conclude if the user has finished a drawing (e.g. a letter) the programm needs to wait a certain amount of time as only detecting if the input has stopped once does not mean a letter is finished (e.g. "i",　"お"). In user testings those timings need to be refined.

## Scenario III Circular moving virtual surface
In the second experiment the moving virtual surface is changed to a circular form as can be seen in the following graphic.

![image](https://user-images.githubusercontent.com/116259509/199899611-b12809c7-b387-4edf-bcba-35ab4aba4ec6.png)

## Scenario IV Scalable Drawing

![image](https://user-images.githubusercontent.com/116259509/199903030-d55a780c-72d9-48fc-83d5-5d7470e94c19.png)

![image](https://user-images.githubusercontent.com/116259509/199903078-f6f28a58-74fa-4bdb-8294-7a16f6c59fe7.png)
