```
Author: Denis Lebó
Last Modification Date: 20/05/2022
```
# SettleDown API Documentation

All endpoints have the following form: `/api/[Endpoint]`. For brevity, in the following chapters only the **Endpoint**
will be given. This API is supposed to serve as a backend for a future summer project of developing an app for group
fund management. It was developed as a part of the subject `PV178 - Úvod do vývoje v C#/.NET`.

## Group
Available methods: `[ GET, POST ]`

**Access privilege is determined by data contained in the JWT token.
If the token is out-of-date and does not contain the most up-to-date data, some groups may not be visible.**

#### GET
Retrieves all group the current user has access to.

#### POST 
Creates a new group. Unless importing data from a previously existing group  *members* and *debt* fields should not be 
included. If the current user is not present in the *members* field, he is automatically added.

***After this operation you MUST request a new JWT token to have access to the created group!***

## Group/{id}
**(int) id  - id of the group**

Available methods: `[ GET, PUT, DELETE ]`

**Access privilege is determined by data contained in the JWT token.
If the token is out-of-date and does not contain the most up-to-date data, some groups may not be accessible.**


#### GET
Retrieves the group with details of members and debt.

#### PUT 
Updates the name of the group. 

#### DELETE
Deletes the group

## Member

Available methods: `[ GET, POST ]`

**Access privilege is determined by data contained in the JWT token.
If the token is out-of-date and does not contain the most up-to-date data, some members may not be visible.**

#### GET

Retrieves all members the current user has access to.

#### POST

Creates a new member

## Member/{id}
**(int) id  - id of the member**

Available methods: `[ GET, PUT ]`

**Access privilege is determined by data contained in the JWT token.
If the token is out-of-date and does not contain the most up-to-date data, some groups may not be accessible.**

#### GET
Retrieves the member

#### PUT
Updates the name of the member.

## Register

Available methods: `[ POST ]`

#### POST
Creates a user account. All access privileges are bound to this user account!

## Token

Available methods: `[ POST ]`

#### POST
Generates a an up-to-date JWT token with necessary access information

## Transaction

Available methods: `[ GET, POST ]`

**Access privilege is determined by data contained in the JWT token.
If the token is out-of-date and does not contain the most up-to-date data, some transactions may not be visible.**

#### GET 

Retrieves all transactions that the user has paid for.

#### POST

Creates a new transaction.

If *debts* is left empty, the cost will be dispersed equally among all members.
When defining *weight* or *amount*, both **CANNOT** be defined in the same debt. However, it can be used in combination
between different debts. E.g. debt 1 has *amount* and debt 2 has *weight*. When determining amounts and weights
first all *amount* debts will be allocated and then the residual cost is split between *weight* debts.

## Transaction/{id}
**(int) id  - id of the transaction**

Available methods: `[ GET, PUT ]`

**Access privilege is determined by data contained in the JWT token.
If the token is out-of-date and does not contain the most up-to-date data, some groups may not be accessible.**

#### GET

Retrieves the transaction

#### PUT

Update the name of the transaction