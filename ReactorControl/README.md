# Reactor Controller 
This Windows Forms application serves as the interface between a user and the reactor microcontroller. This application is responsible for:
- Sending test specifications to the microcontroller
- Collecting data from the microcontroller
- Displaying the state of the interlocks (door open, overheat sensor, ect).

## Program.cs
This is where the main lives. Responsibilities include:
- Starting the application
- Registers the classes for dependency injection\
- Handling the disposal of the application.

## MainForm.cs
This is where the UI and business logic interface with eachother. Responsibilities include:
- Handling click events to start a particular action 
	- Ex: (connect button tells the ComPortManager to connect with active COM port)
- Logging and reflecting the state of the microcontroller and running test
- Catching exceptions thrown from other classes to display or handle them appropriately

## ComPortManager.cs 
This is where the application interfaces with the microcontroller. Responsibilites include:
- Creating the serial object with the correct specifications
- Handling the connection, disconnection, and streaming of data to and from the microcontroller
- Raising events whenever data is sent from the microcontroller
- Catching command events from the TestManager
- Throwing exceptions whenever an error occurs 

## TestManager.cs
This is where the test specific business logic runs. Responsibilities include: 
- Catching data events from the ComPortManager, and packaging it to be more useable
- Creating command events and passing them to the ComPortManager
- Exporting test data to a CSV file stored on the users desktop
- Clearing the test data

## Other Info:
- Both the microcontroller and the Reactor Controller expect this data stream format:
	- command:command_checksum:enrichments:end_byte:\r\n
	- enrichments can be test data, in which the format is:
		- probe_temperature:wall_power:magnetron_power:reflected_power:
	- enrichments can also be test specifications, in which the format is:
		- target_temp:delta_temp:temp_hold_time: