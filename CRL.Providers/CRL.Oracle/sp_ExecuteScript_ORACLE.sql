--��ִ̬�����洢����
create or replace procedure sp_ExecuteScript(script in varchar2) Authid Current_User as
begin
  execute immediate script;
end;