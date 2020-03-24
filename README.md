# CRL.NetStandard

此项目为.NetStandard库,可同时运行于.net Framework和.net Core
CRL系列包含ORM,API,EventBus,RPC(socket,gRPC)等

基本涵盖了一般开发所需的各种需求,如:
-  数据源切换
- 数据缓存
- 数据库结构同步
- 复杂数据查询
- 读写分离方案
- 分库分表方案
- API调用/实现与微服务集成,认证
- RPC/gRPC实现与微服务集成,认证
- EventBus与数据队列处理

ORM: 支持多种数据库(mssql,oracle,mysql),支持复杂查询(关联,嵌套),多种缓存实现方式(内存,redis),自动表结构检查/维护,仓储和DbSet模式,通过动态数据源实现读写分离和自定义分库分表,非关系型数据库支持(MongoDB)等

API代理: 通过类型动态代理(DynamicProxy),只需定义API接口(interface)契约,实现API接口的调用,简化了开发流程,内部集成了consul服务发现和ocelot网关,认证和polly策略

动态API: 包含HTTP API和RPC,集成了consul服务发现和ocelot网关,认证和polly策略

gPRC扩展: 集成了consul服务发现,认证和polly策略

EventBus: 支持MongoDb,RabbitMQ,Redis,支持批量消息发布和批量消息订阅,也可以作为任务处理队列使用

API相关说明:https://www.cnblogs.com/hubro/p/11652687.html

ORM相关说明:https://www.cnblogs.com/hubro/p/12124687.html
