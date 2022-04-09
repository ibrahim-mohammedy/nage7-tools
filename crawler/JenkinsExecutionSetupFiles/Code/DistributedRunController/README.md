
# DistributedRunController

This is a console app which is used for breaking a UIAutomation solutions Test Suite into small chunks and distributing execution over a multi-node Jenkins automation execution setup. This is primarily done using jenkinsnet api. This solution is run in 3 modes:

## **master:** 
When run in this configuration. The solution performs the following tasks:

 1. Breaks down the execution config xml file (TestSuite) into smaller execution config files and places them all in a shared path accessible by all the nodes
 2. Runs build of the solution (UIAutomation) on all the slave VMs
 3. Initiates execution of UIAutomation solution on the slave VM nodes with one small execution config group per node
 4. Keeps track of the completed execution config groups and waits for the slave node job completion to assign pending groups

*Parameters (in order):*  
 1. appRole=master
 2. slaves=\[jenkinks url for the slave nodes\]
 3. env=\[environment to test\]
 4. browser=\[Chrome/Firefox/IE\]
 5. config=\[ExecutionConfig xml (Test Suite)\]
 6. codeBranch=\[specific branch to run (default to master)\]
 7. host=\[jenkins url for the master\]
 8. share=\[shared location accessible to all slave VMs\]
 9. threads=\[# of threads to run on each VM\]

## **slave_init:**
When run in this configuration. The solution performs the following tasks:

 1. After execution completes for a group on the slave nodes, this job uploads the results of the execution to the shared folder for master VM to consolidate the results

*Parameters (in order):*  
1. appRole=slave
2. host=\[jenkins url for the master\]
3. share=\[shared location accessible to all slave VMs\]
  
 
## **slave:**
When run in this configuration. The solution performs the following tasks:

 1. After build completion on the slave nodes, this job creates a folder in the root dir of UIAutomation (on slave nodes) and places all the smaller execution config file

*Parameters (in order):*  
1. appRole=slave
2. group=\[small execution config group that was run\]
3. host=\[jenkins url for the master\]
4. share=\[shared location accessible to all slave VMs\]
5. workspace=\[workspace path for the slave jenkins job\]