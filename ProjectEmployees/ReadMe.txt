Projects description:
	ProjectEmployees.Core - meant to execute the main logic and be included in different applications as a reference.
	ProjectEmployees.WebAPI - application entry point using swagger for documentation and execution.
	ProjectEmployeesConsole - application entrypoint meant for testing main logic.

Description:
	At first I ambitiosly decided to make the project into something that could be expanded upon.
	That created a few issues for the time of development as it left me with more time to plan and less time to develop.
	The issues from that is that I did not manage to make a custom UI for this purpose.
		The plan for the UI was a simple Angular app that would show 1 file upload input and 2 tables.
		First table to be for all uploaded files and a button to display the results from them.
		Second table to display the results for selected process.
	
	Despite the requirements telling that only the highest shared time should be displayed, my code actually provides a list of all shared times sorted in descending order based on the amount of shared time.
	My initial idea was to have another method to just get the top item from this collection.
	
	For a UI, I decided to implement swagger for functional access as I would have had to break my deadline to work on the previously thought idea.
	
	The way it works is:
		Get CSV and read it.
		Check if the columns order is as expected or changed.
		Separate the contents into different lines and convert the raw strings into useable objects.
		Segregate all entries into different collections based on the project id
		Pair up all employees that have crossing work times on the project into new collections, segregated in the same way as above.
		Combine all coinciding entries (same employee pair) by adding up their shared times and remove all repeating entries.
		Break the segregation and return all employee pairs into a single collection.
	
	Missed issues:
		Check if CSV is in the correct format, though some checks do demand data integrity
		Some error handling can be better
		Check for work time inconcistencies (employee dateto on a project is after different entry datefrom)

How to use:
	Load swagger UI
	Use upload endpoint to upload a csv file.
	Use GetFiles endpoint to get list of uploaded files.
	Use GetData endpoint to get list of employee pairs for projects and their shared time. 
		Provided argument has to be an item from the previous step.
		File extension included in provided argument.